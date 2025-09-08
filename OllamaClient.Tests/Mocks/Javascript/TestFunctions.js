const fs = require('fs');

function Test1(num1, num2) {
	const result = num1 + num2;
	fs.writeFileSync('result.txt', `The result is: ${result}`);
	return result;
}

function Test2(text) {
	console.log(text);
}

module.exports = {
	Test1: Test1,
	Test2: Test2
};