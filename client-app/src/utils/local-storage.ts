export const putStorageObject = <T extends object>(key: string, object: T) => {
  const stringified = JSON.stringify(object)
  localStorage.setItem(key, stringified)
}

export const getStorageObject = <T extends object>(key: string) => {
  const stringifiedObject = localStorage.getItem(key)
  const object = stringifiedObject ? (JSON.parse(stringifiedObject) as T) : undefined
  return object
}

export const updateStorageObject = <T extends object>(
  key: string,
  updateFunction: (obj: T) => T | void,
  defaultObject: T
) => {
  const object = getStorageObject<T>(key) ?? defaultObject
  const updateFunctionReturn = updateFunction(object)
  putStorageObject<T>(key, updateFunctionReturn !== undefined ? updateFunctionReturn : object)
}
