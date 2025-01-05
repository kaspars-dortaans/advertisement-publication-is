export const debounceFn = (callback: Function, debounceTime: number) => {
  let timeoutId: number | undefined = undefined

  const clear = () => {
    if (timeoutId) {
      clearTimeout(timeoutId)
    }
  }
  const debounce = (...args: unknown[]) => {
    clear()
    timeoutId = setTimeout(() => callback(args), debounceTime)
  }

  return { debounce, clear }
}
