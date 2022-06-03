import * as array from './arrayHelper'
import * as dictionary from './dictionaryHelper'
import * as string from './stringHelper'
import * as dateTime from './dateTimeHelper'
import { enumStringToInt, enumIntToString, tryCoerceEnum } from '../metadata/enums'
import { getEnumMetadata } from '../metadata/metadataHelper'


const enums = {
	fromInt(value: any, type: string) {
		return tryCoerceEnum(value, getEnumMetadata(type)).value
	},
	toInt(value: any, type: string) {
		return enumStringToInt(value, getEnumMetadata(type))
	}
}


export default Object.freeze({
	array,
	dictionary,
	string,
	dateTime,
	enums
})
