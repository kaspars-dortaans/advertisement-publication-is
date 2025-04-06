import { axiosInstance } from '@/init/axios'
import { LocaleService } from '@/services/locale-service'
import { HttpStatusCode } from 'axios'

const ls = LocaleService.get()

export const downloadFile = async (url: string) => {
  const response = await axiosInstance.get(url, {
    responseType: 'blob'
  })

  if (response.status === HttpStatusCode.Ok) {
    const contentDisposition = response.headers['content-disposition'] as string
    const fileName = contentDisposition.match(/filename\s*=(.+);/)?.[1] ?? ''
    return new File([response.data], fileName)
  }
}

/**
 * Hash file
 * @param file
 * @return file hash in hexadecimal string format
 */
export const hashFile = async (file: File) => {
  const fileArrayBuffer = await file.arrayBuffer()
  const fileData = new Uint8Array(fileArrayBuffer)
  const hash = await hashUint8Array(fileData)
  return uint8ArrayToHexString(new Uint8Array(hash))
}

/**
 * Hash unsigned 8 bit int array
 * @param array
 * @returns hash array buffer
 */
export const hashUint8Array = (array: Uint8Array) => {
  return window.crypto.subtle.digest('SHA-256', array)
}

/**
 * Convert unsigned 8 bit int array to hexadecimal string
 * Based on: https://stackoverflow.com/questions/60595630/javascript-use-input-type-file-to-compute-sha256-file-hash
 * @param array
 * @returns hexadecimal string
 */
export const uint8ArrayToHexString = (array: Uint8Array) => {
  let hexString = '',
    h
  for (let i = 0; i < array.length; i++) {
    h = array[i].toString(16)
    if (h.length == 1) {
      h = '0' + h
    }
    hexString += h
  }
  const p = Math.pow(2, Math.ceil(Math.log2(hexString.length)))
  hexString = hexString.padStart(p, '0')
  return hexString.toUpperCase()
}

/** Detect data size unit and format to it */
export const formatDataSize = (sizeInBytes: number, maxDecimalPlaces = 2) => {
  const sizeUnits = ['bits', 'bytes', 'kb', 'mb', 'gb', 'tb']
  let detectedUnitIndex, sizeInUnit
  if (sizeInBytes > 0) {
    detectedUnitIndex = sizeUnits.length - 1
    for (let i = 1; i < sizeUnits.length; i++) {
      //If value is smaller than 0 in current data size unit set previous unit as detected unit
      if (sizeInBytes / Math.pow(1024, i - 1) < 1) {
        detectedUnitIndex = i - 1
        break
      }
    }
  } else {
    detectedUnitIndex = 0
  }

  if (detectedUnitIndex === 0) {
    sizeInUnit = (sizeInBytes / 8).toFixed(0)
  } else {
    //detectedUnitIndex - 1 is used to account for 'bits' in sizeUnits array
    sizeInUnit = sizeInBytes / Math.pow(1024, detectedUnitIndex - 1)
    if (!Number.isInteger(sizeInUnit)) {
      sizeInUnit = sizeInUnit.toFixed(maxDecimalPlaces)
      //Remove trailing comma and zeros
      const match = sizeInUnit.match(/\.?0*$/)
      if (match?.length && match.length > 0) {
        sizeInUnit = sizeInUnit.slice(0, sizeInUnit.length - match[0].length)
      }
    }
  }

  return ls.l('dataSize.' + sizeUnits[detectedUnitIndex], sizeInUnit)
}
