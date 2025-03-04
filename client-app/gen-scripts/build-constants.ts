import fs from 'fs'
import path from 'path'

/** Types to store information about found classes and its constants */
type Constant = {
  name: string
  value: string
}

type Class = {
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

/** Extract public classes with declared and assigned public constants */
const extractConstants = async (
  filePath: string,
  requiredClassKeywords: string[],
  requiredFieldKeywords: string[]
) => {
  //Result array
  const classesWithConstants: Class[] = []

  const code = await readFile(filePath)
  //Last list element represents for which class current statement belong. Previous elements are parent classes for nested classes.
  const classStack: Class[] = []
  const codeBlockStartIndexStack: number[] = []

  let insideString = false
  //Holds " or ' if currently in string or char value respectively
  let stringChar = ''
  let lastStatementEndIndex = -1

  //Walk through file and extract classes and its constants
  for (let i = 0; i < code.length; i++) {
    switch (code[i]) {
      case ';':
        if (insideString) {
          break
        }

        //If currently not in class scope skip (function scope is skipped)
        if (
          classStack.length &&
          codeBlockStartIndexStack.length === classStack[classStack.length - 1].codeBlockLevel
        ) {
          //Plus one to not include previous statement last char, e.g. ; or }
          const statement = code.slice(lastStatementEndIndex + 1, i).trim()
          //Regex is used to not split on arrow functions
          const assignment = statement.split(/[a-zA-Z0-9\s]=[a-zA-Z0-9\s'"]/)

          //Skip if current statement is not declaration & assignment statement
          if (assignment.length > 1) {
            const tokens = assignment[0].split(/\s/)

            if (
              tokens.length >= requiredFieldKeywords.length + 1 && //plus field name
              requiredFieldKeywords.every((keyword) => tokens.some((t) => t === keyword))
            ) {
              classStack[classStack.length - 1].constants.push({
                name: tokens[tokens.length - 1],
                //statement is used to preserve value if it was also split by regex
                value: statement.slice(assignment[0].length + 3) //plus regex match length
              })
            }
          }
        }

        lastStatementEndIndex = i
        break

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
            const foundClass: Class = {
              name: tokens[classKeywordIndex + 1],
              constants: [],
              codeBlockStartIndex: i,
              codeBlockLevel: codeBlockStartIndexStack.length
            }
            classStack.push(foundClass)
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
        //Skip if not currently in class || current class start block index is not equal current code block start index
        if (
          classStack.length &&
          classStack[classStack.length - 1].codeBlockStartIndex === codeBlockStartIndex
        ) {
          //If class had constants found add it to result array
          const closedClass = classStack.pop()
          if (closedClass?.constants.length) {
            classesWithConstants.push(closedClass)
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
    }
  }
  return classesWithConstants
}

/** Generate Ts file which contains class with constants */
const generateConstantTsClassFile = async (destinationPath: string, c: Class) => {
  const fields = c.constants.map((c) => {
    return `  static readonly ${c.name} = ${c.value}`
  })
  const fileContents = 'export class ' + c.name + ' {\n' + fields.join('\n') + '\n}'

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
    extractConstants(filePath, c.requiredClassKeywords, c.requiredFieldKeywords)
  )
  const classesWithConstants = (await Promise.all(constantPromises)).flatMap((c) => c)

  //Generate Ts file from extracted data
  const generationPromises: Promise<void>[] = []
  for (const constantClass of classesWithConstants) {
    generationPromises.push(generateConstantTsClassFile(c.destinationFolder, constantClass))
  }
  await Promise.all(generationPromises)
}

const config: GenerateConstantsConfig = {
  destinationFolder: './src/constants/api/',
  apiProjectPath: '../api/',
  excludeFolders: ['.vs', 'bin', 'obj'],
  allowedExtensions: ['.cs'],
  requiredClassKeywords: ['public', 'class'],
  requiredFieldKeywords: ['public', 'const']
}

generateConstants(config)
