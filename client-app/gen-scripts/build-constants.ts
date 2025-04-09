import fs, { RmDirOptions } from 'fs'
import path from 'path'

/** Types to store information about found classes and its constants */
type Constant = {
  name: string
  value: string
  comment: boolean
}

type ConstantCollection = {
  type: 'class' | 'enum'
  name: string
  constants: Constant[]
  codeBlockStartIndex: number
  codeBlockLevel: number
}

type GenerateConstantsConfig = {
  /** Relative path to destination folder for generated constant files */
  destinationFolder: string

  /** Relative Api project path on file system */
  apiProjectPath: string

  /** List of folder names to exclude when scanning Api project */
  excludeFolders: string[]

  /** Allowed file extensions when scanning Api project*/
  allowedExtensions: string[]

  /** Required keywords for class to be allowed to generate for client */
  requiredClassKeywords: string[]

  /** Required keywords for field to be allowed to generate for */
  requiredFieldKeywords: string[]

  /** Required keywords for enums */
  requiredEnumKeywords: string[]
}

/** Wrap fs.readDir in new promise */
const readFolder = (dirPath: string): Promise<string[]> => {
  return new Promise((resolve) => fs.readdir(dirPath, (_, files) => resolve(files)))
}

/** Wrap fs.readFile in new promise */
const readFile = (filePath: string) => {
  return new Promise<string>((resolve) =>
    fs.readFile(filePath, 'utf-8', (_, data) => resolve(data))
  )
}

/** Wrap fs.rmD in new promise */
const rm = (path: string, options: RmDirOptions = {}) => {
  return new Promise<void>((resolve) => fs.rm(path, options, () => resolve()))
}

/** Wrap fs.mkdir in new promise */
const mkdir = (path: string) => {
  return new Promise<void>((resolve) => fs.mkdir(path, () => resolve()))
}

/** Get list of all file in directory and its subdirectories */
const getDirectoryFilesRecursive = async (
  dirPath: string,
  excludeFolders: string[] = [],
  allowedExtensions?: string[]
) => {
  const nameList = await readFolder(dirPath)
  const fileList: string[] = []
  const readDirectoryPromises: Promise<void>[] = []
  for (const name of nameList) {
    const itemPath = path.join(dirPath, name)

    //Handle directory item when its info is loaded
    const handleItem = async (resolve: () => void, stats: fs.Stats) => {
      if (stats.isDirectory() && !excludeFolders.some((folderName) => folderName === name)) {
        const directoryFileList = await getDirectoryFilesRecursive(
          itemPath,
          excludeFolders,
          allowedExtensions
        )
        fileList.push(...directoryFileList)
      } else if (
        stats.isFile() &&
        (!allowedExtensions?.length || allowedExtensions.some((ext) => name.endsWith(ext)))
      ) {
        fileList.push(itemPath)
      }
      resolve()
    }

    //Make call for item info and add its promise to stack
    readDirectoryPromises.push(
      new Promise<void>((resolve) =>
        fs.lstat(itemPath, async (_, stats) => handleItem(resolve, stats))
      )
    )
  }
  await Promise.all(readDirectoryPromises)
  return fileList
}

/** Extract types with declared and assigned public constants */
const extractConstants = async (
  filePath: string,
  requiredClassKeywords: string[],
  requiredFieldKeywords: string[],
  requiredEnumKeywords: string[]
) => {
  //Result array
  const collectionsWithConstants: ConstantCollection[] = []

  const code = await readFile(filePath)
  //Last list element represents for which constantCollection current statement belong. Previous elements are parent collections for nested collections.
  const collectionStack: ConstantCollection[] = []
  const codeBlockStartIndexStack: number[] = []
  const assignmentRegex = /[a-zA-Z0-9\s]=[a-zA-Z0-9\s'"]/

  let insideString = false
  //Holds " or ' if currently in string or char value respectively
  let stringChar = ''
  let lastStatementEndIndex = -1

  //Methods to add new constants to different type collection types
  const addClassConstant = (i: number) => {
    //Plus one to not include previous statement last char, e.g. ; or }
    const statement = code.slice(lastStatementEndIndex + 1, i).trim()
    //Regex is used to not split on arrow functions
    const assignment = statement.split(assignmentRegex)

    //Skip if current statement is not declaration & assignment statement
    if (assignment.length > 1) {
      const tokens = assignment[0].split(/\s/)

      if (
        tokens.length >= requiredFieldKeywords.length + 1 && //plus field name
        requiredFieldKeywords.every((keyword) => tokens.some((t) => t === keyword))
      ) {
        collectionStack[collectionStack.length - 1].constants.push({
          name: tokens[tokens.length - 1], //statement is used to preserve value if it was also split by regex
          value: statement.slice(assignment[0].length + 3), //plus regex match length
          comment: false
        })
      }
    }
  }

  const addEnumConstant = (i: number) => {
    const assignment = code.slice(lastStatementEndIndex + 1, i).split(assignmentRegex)
    if (assignment.length) {
      collectionStack[collectionStack.length - 1].constants.push({
        name: assignment[0].trim(),
        value: assignment[1]?.trim(),
        comment: false
      })
    }
    lastStatementEndIndex = i
  }

  //Walk through file and extract classes and its constants
  for (let i = 0; i < code.length; i++) {
    switch (code[i]) {
      case ';':
        if (insideString) {
          break
        }

        //If currently not in class scope skip (function scope is skipped)
        if (
          collectionStack.length &&
          codeBlockStartIndexStack.length ===
            collectionStack[collectionStack.length - 1].codeBlockLevel
        ) {
          addClassConstant(i)
        }

        lastStatementEndIndex = i
        break

      case ',': {
        if (collectionStack.length && collectionStack[collectionStack.length - 1].type === 'enum') {
          addEnumConstant(i)
        }
        break
      }

      case '{': {
        if (insideString) {
          break
        }

        codeBlockStartIndexStack.push(i)
        //Plus one to not include previous statement last char, e.g. ; or }
        const preBlockStatement = code.slice(lastStatementEndIndex + 1, i).trim()

        if (preBlockStatement.length) {
          const tokens = preBlockStatement.split(/\s/)
          //Check if statement up until code block matches class declaration with required keywords
          if (
            tokens.length >= requiredClassKeywords.length &&
            requiredClassKeywords.every((keyword) => tokens.some((t) => t === keyword))
          ) {
            const classKeywordIndex = tokens.indexOf('class')
            const foundCollection: ConstantCollection = {
              type: 'class',
              name: tokens[classKeywordIndex + 1],
              constants: [],
              codeBlockStartIndex: i,
              codeBlockLevel: codeBlockStartIndexStack.length
            }
            collectionStack.push(foundCollection)

            //Check if statement up until code block matches enum declaration with required keywords
          } else if (
            tokens.length >= requiredEnumKeywords.length &&
            requiredEnumKeywords.every((keyword) => tokens.some((t) => t === keyword))
          ) {
            const foundCollection: ConstantCollection = {
              type: 'enum',
              name: tokens[tokens.length - 1],
              constants: [],
              codeBlockStartIndex: i,
              codeBlockLevel: codeBlockStartIndexStack.length
            }
            collectionStack.push(foundCollection)
          }
        }
        lastStatementEndIndex = i
        break
      }

      case '}': {
        if (insideString) {
          break
        }

        const codeBlockStartIndex = codeBlockStartIndexStack.pop()
        //Skip if not currently in constant collection || current constant collection start block index is not equal current code block start index
        if (
          collectionStack.length &&
          collectionStack[collectionStack.length - 1].codeBlockStartIndex === codeBlockStartIndex
        ) {
          //If in enum add last enum value, if it is present
          if (
            collectionStack.length &&
            collectionStack[collectionStack.length - 1].type === 'enum' &&
            code.slice(lastStatementEndIndex + 1, i).match(/[a-zA-Z]/)
          ) {
            addEnumConstant(i)
          }

          //If constant collection had constants found add it to result array
          const closedCollection = collectionStack.pop()
          if (closedCollection?.constants.length) {
            collectionsWithConstants.push(closedCollection)
          }
        }
        lastStatementEndIndex = i
        break
      }

      //Handle string values
      case '"':
        if (!insideString) {
          insideString = true
          stringChar = '"'
        } else if (stringChar === '"' && code[Math.max(i - 1, 0)] !== '\\') {
          insideString = false
        }
        break

      //Handle char values
      case "'":
        if (!insideString) {
          insideString = true
          stringChar = "'"
        } else if (stringChar === "'" && code[Math.max(i - 1, 0)] !== '\\') {
          insideString = false
        }
        break

      case '\n':
        if (
          collectionStack.length &&
          codeBlockStartIndexStack.length ===
            collectionStack[collectionStack.length - 1].codeBlockLevel
        ) {
          const statement = code.slice(lastStatementEndIndex + 1, i)
          const commentStart = statement.lastIndexOf('//')
          if (commentStart > -1) {
            const comment = statement.slice(commentStart, i)
            lastStatementEndIndex = i
            collectionStack[collectionStack.length - 1].constants.push({
              name: '',
              value: comment,
              comment: true
            })
          }
        }
        break
    }
  }

  return collectionsWithConstants.filter((collection) =>
    collection.constants.some((c) => !c.comment)
  )
}

/** Generate Ts file which contains constants */
const generateConstantTsFile = async (destinationPath: string, c: ConstantCollection) => {
  let fields: string[] = []
  let joinString = ''

  switch (c.type) {
    case 'class':
      fields = c.constants.map((c) => {
        return c.comment ? '  ' + c.value : `  static readonly ${c.name} = ${c.value}`
      })
      joinString = '\n'
      break
    case 'enum':
      fields = c.constants.map((c) => {
        return c.comment
          ? '  ' + c.value
          : `  ${c.name}${c.value !== undefined ? ` = ${c.value},` : ','}`
      })
      joinString = '\n'
      break
  }

  const fileContents = `export ${c.type} ${c.name} {\n${fields.join(joinString)}\n}`

  return new Promise<void>((resolve) =>
    fs.writeFile(path.join(destinationPath, c.name + '.ts'), fileContents, () => resolve())
  )
}

/** Read constants from c# Api project and generate Ts files with found constants in frontend project */
const generateConstants = async (c: GenerateConstantsConfig) => {
  //Get c# file list
  const cSharpFilePaths = await getDirectoryFilesRecursive(
    c.apiProjectPath,
    c.excludeFolders ?? [],
    c.allowedExtensions
  )

  //Extract class and their constant data
  const constantPromises = cSharpFilePaths.map((filePath) =>
    extractConstants(
      filePath,
      c.requiredClassKeywords,
      c.requiredFieldKeywords,
      c.requiredEnumKeywords
    )
  )
  const classesWithConstants = (await Promise.all(constantPromises)).flatMap((c) => c)

  //Remove old files
  await rm(c.destinationFolder, { recursive: true })

  //Recreate directory
  await mkdir(c.destinationFolder)

  //Generate Ts file from extracted data
  const generationPromises: Promise<void>[] = []
  for (const constantClass of classesWithConstants) {
    generationPromises.push(generateConstantTsFile(c.destinationFolder, constantClass))
  }
  await Promise.all(generationPromises)
}

const config: GenerateConstantsConfig = {
  destinationFolder: './src/constants/api/',
  apiProjectPath: '../api/',
  excludeFolders: ['.vs', 'bin', 'obj'],
  allowedExtensions: ['.cs'],
  requiredClassKeywords: ['public', 'class'],
  requiredFieldKeywords: ['public', 'const'],
  requiredEnumKeywords: ['public', 'enum']
}

generateConstants(config)
