/* eslint-disable @typescript-eslint/no-explicit-any */
import fs from 'fs'

import { DefaultLocaleName } from '../src/constants/default-locale'

const localePath = 'src/locales/'

type PropertyDescriptor = {
  depth: number
  parentName?: string
  value?: string
  name: string
  isArray: boolean
  entries: [string, any][]
}

const buildLocales = async () => {
  const defaultLocale: object = await new Promise((resolve) =>
    fs.readFile(`${localePath}/${DefaultLocaleName}.json`, 'utf8', (_, data) =>
      resolve(JSON.parse(data))
    )
  )
  buildEmptyLocale(defaultLocale)
  matchOtherLocaleStructure(defaultLocale)
}

/** Create ts file with exported js object which matches default locale structure, but with empty values */
const buildEmptyLocale = (locale: object) => {
  const emptyLocaleFileContents =
    '/** This is auto generated file\n' +
    '* Assign empty locale to prevent errors when trying to localize while real locale is not loaded */\n' +
    'export const emptyLocale = ' +
    stringifyJsObjectEmpty(locale)

  fs.writeFile('src/init/empty-locale.ts', emptyLocaleFileContents, () => {})
}

/**
 * Stringify object, but all properties of type not equal to 'object' will be assigned empty string value
 * @param obj Object to stringify
 * @returns String representation of object
 */
const stringifyJsObjectEmpty = (obj: object) => {
  const objectProperties: PropertyDescriptor[] = [
    { depth: 0, parentName: '', name: '', entries: Object.entries(obj).reverse(), isArray: false }
  ]
  const resultStrings: string[] = []
  const singleIndentation = '  '

  let property: PropertyDescriptor | undefined
  let lastProperty: PropertyDescriptor = {
    depth: 0,
    name: 'NoName',
    entries: [],
    isArray: false,
    parentName: 'NoParentName'
  }
  while ((property = objectProperties.pop())) {
    const indentation = singleIndentation.repeat(property.depth)
    const openBracket = property.isArray ? '[' : '{'
    const closingBracket = lastProperty.isArray ? ']' : '}'
    if (property.depth < lastProperty.depth || lastProperty.parentName === property.parentName) {
      printClosingBrackets(lastProperty.depth, property.depth, true, resultStrings, closingBracket)
    }

    resultStrings.push(
      indentation + (property.name ? property.name + ': ' : '') + `${openBracket}\n`
    )
    if (property.entries) {
      for (let i = 0; i < property.entries.length; i++) {
        if (
          typeof property.entries[i][1] === 'object' &&
          property.entries[i][1] != null &&
          !property.isArray
        ) {
          objectProperties.push({
            depth: property.depth + 1,
            parentName: property.name,
            name: property.entries[i][0],
            entries: Object.entries(property.entries[i][1]),
            isArray: Array.isArray(property.entries[i][1])
          })
        } else {
          resultStrings.push(
            indentation +
              singleIndentation +
              `${property.isArray ? '' : property.entries[i][0] + ':'} ''${(lastProperty.depth === 0 && objectProperties.length) || i < property.entries.length - 1 ? ',' : ''}\n`
          )
        }
      }
    }

    lastProperty = property
  }

  if (lastProperty.depth) {
    printClosingBrackets(lastProperty.depth, 0, false, resultStrings)
  }

  return resultStrings.join('')
}

const printClosingBrackets = (
  fromDepth: number,
  toDepth: number,
  addComma: boolean,
  resultArray: string[],
  bracket = '}'
) => {
  const closingBracketString = bracket + (addComma ? ',\n' : '\n')
  for (let i = fromDepth; i >= toDepth; i--) {
    const indentation = '  '.repeat(i)
    resultArray.push(indentation + closingBracketString)
  }
}

/** Inspect other locale and make sure that their structure matches default locale structure
 * Missing properties will be added with placeholder values, values which are not present in default locale will be removed
 */
const matchOtherLocaleStructure = async (defaultLocale: object) => {
  //Load other locales
  const fileList: string[] = await new Promise((resolve) =>
    fs.readdir(localePath, 'utf-8', (_, files) => resolve(files))
  )

  const localePromises: Promise<{ fileName: string; locale: object }>[] = fileList
    .filter((fileName) => fileName.endsWith('.json') && !fileName.startsWith(DefaultLocaleName))
    .map(
      (fileName) =>
        new Promise((resolve) =>
          fs.readFile(`${localePath}/${fileName}`, 'utf-8', (err, data) =>
            resolve({ fileName: fileName, locale: JSON.parse(data) })
          )
        )
    )

  const saveLocalePromises: Promise<void>[] = []
  //Loop over them and match structure
  for (const localePromise of localePromises) {
    const localeInfo = await localePromise
    const matchedLocale = matchObjectStructure(defaultLocale, localeInfo.locale)
    const str = JSON.stringify(matchedLocale, null, 2) + '\n'
    saveLocalePromises.push(
      new Promise<void>((resolve) =>
        fs.writeFile(`${localePath}/${localeInfo.fileName}`, str, () => resolve())
      )
    )
  }

  await Promise.all(saveLocalePromises)
}

/** Convert entry list to PropertyDescriptor list */
const convertToDescriptor = (
  entries: Record<string, any>[],
  parentDepth: number
): PropertyDescriptor[] => {
  return entries.map((e) => ({
    depth: parentDepth + 1,
    entries: typeof e[1] === 'object' ? Object.entries(e[1]) : [],
    isArray: Array.isArray(e[1]),
    name: e[0],
    value: e[1]
  }))
}

/** Copies object structure from sourceObj and fills it with corresponding values from targetObj when they are present */
const matchObjectStructure = (sourceObj: object, targetObj: object) => {
  const sourceProperties: PropertyDescriptor[] = convertToDescriptor(
    Object.entries(sourceObj).reverse(),
    0
  )
  const targetProperties: PropertyDescriptor[] = convertToDescriptor(
    Object.entries(targetObj).reverse(),
    0
  )
  const missingResourcePlaceholder = 'Todo: Translate'
  const resultObject: object = {}
  const resultObjectPropertyStack = [resultObject]

  let sourceProperty = sourceProperties.pop()
  let targetProperty: PropertyDescriptor | undefined
  let lastDepth = 1
  while (sourceProperty != null) {
    const targetPropertyIndex = targetProperties.findIndex(
      (p) => p.name === sourceProperty?.name && p.depth === sourceProperty.depth
    )
    if (targetPropertyIndex > -1) {
      targetProperty = targetProperties[targetPropertyIndex]
      targetProperties.splice(targetPropertyIndex, 1)
    } else {
      targetProperty = undefined
    }

    if (sourceProperty.depth < lastDepth) {
      //Remove leftover target properties at exited depth
      const depthIndex = targetProperties.findIndex((p) => p.depth === lastDepth)
      if (depthIndex > -1) {
        targetProperties.splice(depthIndex)
      }

      //Free result object property stack to current depth
      resultObjectPropertyStack.splice(sourceProperty.depth)
    }

    const resultProperty = resultObjectPropertyStack[resultObjectPropertyStack.length - 1]
    if (sourceProperty.entries.length) {
      sourceProperties.push(
        ...convertToDescriptor(sourceProperty.entries.reverse(), sourceProperty.depth)
      )

      // resultObject assign object value
      const newProperty = sourceProperty.isArray ? [] : {}
      resultObjectPropertyStack[resultObjectPropertyStack.length - 1][sourceProperty.name] =
        newProperty
      resultObjectPropertyStack.push(newProperty)

      if (targetProperty?.entries.length) {
        targetProperties.push(
          ...convertToDescriptor(targetProperty.entries.reverse(), targetProperty.depth)
        )
      }
    } else {
      // result Object assign string value
      resultProperty[sourceProperty.name] =
        targetProperty != null ? targetProperty.value : missingResourcePlaceholder
    }

    lastDepth = sourceProperty.depth
    sourceProperty = sourceProperties.pop()
  }
  return resultObject
}

buildLocales()
