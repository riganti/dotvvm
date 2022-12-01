export function isNumber(value: any): boolean {
	// soooo, this awesome language does not have a reasonable function to check if an object is number or a numeric string...
	// Number(value) returns NaN for anything which is not a number, except for empty/whitespace string which is converted to 0 🤦
	// parseFloat correctly fails for empty string, but parseFloat("10dddd") returns 10, not NaN

	// so value is number of Number(value) and parseFloat(value) return the same number
	// if both return NaN, === is false, finally this "feature" is useful for something :D
	return parseFloat(value) === Number(value);
}
