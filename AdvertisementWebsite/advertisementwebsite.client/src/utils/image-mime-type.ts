import { LocaleService } from '@/services/locale-service'

const ls = LocaleService.get()
// Based on: https://advanced-cropper.github.io/vue-advanced-cropper/guides/recipes.html#load-image-from-a-disc
/**
 * This function is used to detect the actual image type
 */
export const getImageType = async (file: Blob, fallback: string = '', format = 'image/{0}') => {
  const arrayBuffer = await file.arrayBuffer()
  const byteArray = new Uint8Array(arrayBuffer).subarray(0, 4)
  let header = ''
  for (let i = 0; i < byteArray.length; i++) {
    header += byteArray[i].toString(16)
  }

  let fileType = fallback
  switch (header) {
    case '89504e47':
      fileType = 'png'
      break
    case '47494638':
      fileType = 'gif'
      break
    case 'ffd8ffe0':
    case 'ffd8ffe1':
    case 'ffd8ffe2':
    case 'ffd8ffe3':
    case 'ffd8ffe8':
      fileType = 'jpeg'
      break
  }
  return ls.f(format, [fileType])
}
