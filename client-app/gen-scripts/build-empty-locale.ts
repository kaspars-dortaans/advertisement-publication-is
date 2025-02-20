import fs from 'fs'

import { DefaultLocaleName } from '../src/constants/default-locale'

//Read default locale
fs.readFile(`src/locales/${DefaultLocaleName}.json`, 'utf8', function (err, data) {
  if (err) throw err
  buildEmptyLocale(JSON.parse(data))
})

/** Create ts file with exported js object which matches default locale structure, but with empty values */
const buildEmptyLocale = (locale: object) => {
  const emptyLocaleFileContents =
    '/** This is auto generated file\n' +
    '* Assign empty locale to prevent errors when trying to localize while real locale is not loaded */\n' +
    'export const emptyLocale = ' +
    stringifyObjectEmpty(locale)

  fs.writeFile('src/init/empty-locale.ts', emptyLocaleFileContents, () => {})
}

/**
 * Stringify object, but all properties of type not equal to 'object' will be assigned empty string value
 * @param obj Object to stringify
 * @returns String representation of object
 */
const stringifyObjectEmpty = (obj: object) => {
  type propertyDescriptor = {
    depth: number
    parentName: string
    name: string
    entries: [string, any][]
  }

  const objectProperties: propertyDescriptor[] = [
    { depth: 0, parentName: '', name: '', entries: Object.entries(obj) }
  ]
  const resultStrings: string[] = []
  const singleIndentation = '  '
  let lastDepth = 0
  let lastParentName = 'NoParentName'
  let property: propertyDescriptor | undefined

  while ((property = objectProperties.pop())) {
    const indentation = singleIndentation.repeat(property.depth)
    if (property.depth < lastDepth || lastParentName === property.parentName) {
      printClosingBrackets(lastDepth, property.depth, true, resultStrings)
    }

    resultStrings.push(indentation + (property.name ? property.name + ': {\n' : '{\n'))

    if (property.entries) {
      for (const entry of property.entries) {
        if (typeof entry[1] === 'object' && entry[1] != null) {
          objectProperties.push({
            depth: property.depth + 1,
            parentName: property.name,
            name: entry[0],
            entries: Object.entries(entry[1])
          })
        } else {
          resultStrings.push(indentation + singleIndentation + entry[0] + ': "",\n')
        }
      }
    }

    lastDepth = property.depth
    lastParentName = property.parentName
  }

  if (lastDepth) {
    printClosingBrackets(lastDepth, 0, false, resultStrings)
  }

  return resultStrings.join('')
}

const printClosingBrackets = (
  fromDepth: number,
  toDepth: number,
  addComma: boolean,
  resultArray: string[]
) => {
  const closingBracketString = '}' + (addComma ? ',\n' : '\n')
  for (let i = fromDepth; i >= toDepth; i--) {
    const indentation = '  '.repeat(i)
    resultArray.push(indentation + closingBracketString)
  }
}
