/** Stringify object and put it into local storage. */
export const putStorageObject = <T extends object>(key: string, object: T) => {
  const stringified = JSON.stringify(object)
  localStorage.setItem(key, stringified)
}

/** Retrieve value from locales storage and parse it. */
export const getStorageObject = <T extends object>(key: string) => {
  const stringifiedObject = localStorage.getItem(key)
  const object = stringifiedObject ? (JSON.parse(stringifiedObject) as T) : undefined
  return object
}

/** Update object stored in local storage, with default value to use if object is not found. */
export const updateStorageObject = <T extends object>(
  key: string,
  updateFunction: (obj: T) => T | void,
  defaultObject: T
) => {
  const object = getStorageObject<T>(key) ?? defaultObject
  const updateFunctionReturn = updateFunction(object)
  const updatedObject = updateFunctionReturn !== undefined ? updateFunctionReturn : object
  putStorageObject<T>(key, updatedObject)
  return updatedObject
}
