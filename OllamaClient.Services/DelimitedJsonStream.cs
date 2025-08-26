using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    /// <summary>
    /// A stream reader that reads a stream of JSON objects delimited by a given character
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DelimitedJsonStream<T> : IDisposable where T : notnull
    {
        private StreamReader Reader { get; set; }
        private StringBuilder PartialObject { get; set; }
        private JsonTypeInfo<T> JsonTypeInfo { get; set; }

        public Stream BaseStream { get; set; }
        public char Delimiter { get; set; }
        public bool EndOfStream => Reader.EndOfStream;

        /// <summary>
        /// Create a new DelimitedJsonStream with given BaseStream and Delimiter
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <param name="delimiter"></param>
        public DelimitedJsonStream(Stream sourceStream, char delimiter, JsonTypeInfo<T> jsonTypeInfo)
        {
            PartialObject = new();
            JsonTypeInfo = jsonTypeInfo;
            BaseStream = sourceStream;
            Delimiter = delimiter;
            Reader = new(BaseStream);
        }

        /// <summary>
        /// Disposes the stream reader and the base stream
        /// </summary>
        public void Dispose()
        {
            BaseStream.Dispose();
            Reader.Dispose();
        }

        /// <summary>
        /// Reads the source stream into a string builder and attempts to deserialize the string at each delimiter into an object of type <typeparamref name="T"/>.
        /// Once deserialized, each object is passed to the given IProgress&lt;<typeparamref name="T"/>&gt; parameter via the IProgress&lt;<typeparamref name="T"/>&gt;.Report method.
        /// If the deserialization operation fails, the object is skipped and the next object is attempted.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public async Task Read(IProgress<T> progress, CancellationToken cancellationToken, int bufferSize = 32)
        {
            //buffer
            char[] buffer = new char[bufferSize];
            while (!Reader.EndOfStream)
            {
                //read BaseStream
                await Reader.ReadAsync(buffer, cancellationToken);
                foreach (char c in buffer)
                {
                    //check for delimiter, else append char to PartialObject
                    if (c == Delimiter)
                    {
                        try
                        {
                            //Cleanup any leading characters preceding the opening bracket.
                            //This pattern: " (^.*?{){1} " will match all characters before the first
                            //opening bracket of the json string, including the first opening bracket, only once.
                            string objString = Regex.Replace(PartialObject.ToString(), "(^.*?{){1}", "{");

                            //Serialize PartialObject and pass serialized object to given IProgess parameter if obj not null
                            if (JsonSerializer.Deserialize(objString, JsonTypeInfo) is T obj)
                            {
                                progress.Report(obj);
                            }
                        }
                        catch (JsonException)
                        {
                            //skip object if deserialize operation fails
                            continue;
                        }
                        finally
                        {
                            //clear PartialObject for next object
                            PartialObject.Clear();
                        }
                    }
                    else
                    {
                        //append char to PartialObject
                        PartialObject.Append(c);
                    }
                }
            }
        }
    }
}
