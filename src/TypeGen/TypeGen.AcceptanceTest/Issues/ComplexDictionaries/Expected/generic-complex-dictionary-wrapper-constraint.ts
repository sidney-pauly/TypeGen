/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { ComplexDictionaryKey } from "./complex-dictionary-key";

export class GenericComplexDictionaryWrapperConstraint<TKey extends ComplexDictionaryKey> {
    dict: {Key: TKey, Value: string}[];
    simpleDict: { [key: string]: string; };
    iDict: {Key: TKey, Value: string}[];
}
