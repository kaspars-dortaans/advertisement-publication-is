import type { CategoryItem } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { IFileHashDto } from '@/types/image/file-hash'
import { formatDataSize, hashFile } from '@/utils/file-helper'
import { getImageType } from '@/utils/image-mime-type'
import { isRef, type Ref } from 'vue'
import type { TestFunction } from 'yup'

const ls = LocaleService.get()
const l = LocaleService.currentLocale

/** Validate that advertisements can be added to category */
export const canAddAdvertisementToCategoryValidator = (
  categoryList: Ref<CategoryItem[]>
): TestFunction => {
  return function (id, context) {
    const requiredError = context.createError({
      message: l.value.errors.Required
    })
    if (typeof id !== 'number' || isNaN(id)) {
      return requiredError
    }

    const categoryItem = categoryList.value.find((c) => c.id === id)
    if (!categoryItem) {
      return requiredError
    }
    if (categoryItem.canContainAdvertisements !== true) {
      return categoryList.value.some((c) => c.parentCategoryId == categoryItem.id)
        ? requiredError
        : context.createError({
            message: l.value.errors.CategoryCanNotContainAdvertisements
          })
    }
    return true
  }
}

/** Validate that array element match regex */
export const matchArrayElement = (regexp: RegExp, elementName?: string): TestFunction => {
  return function (_, context) {
    //workaround to get array element value
    const [propertyName, index] = context.path.split(/[[\]]/)
    const value = context?.parent?.[propertyName]?.[index]
    if (value == null) {
      return true
    }

    const match = ('' + value).match(regexp)
    if (!match) {
      return context.createError({
        message: ls.l('errors.FieldNotValid', elementName ?? context.path)
      })
    }
    return true
  }
}

/** Validate file extension */
export const fileType = (allowedFileTypes: string | string[]): TestFunction => {
  if (typeof allowedFileTypes === 'string') {
    allowedFileTypes = allowedFileTypes.split(/[,\s]+/)
  }

  return async function (value, context) {
    if (!(value instanceof File)) {
      return context.createError({
        message: ls.l('errors.FieldNotValid', context.path)
      })
    }

    const fileNameSplit = value.name.trim().split('.')
    const fileExtension = fileNameSplit.length ? fileNameSplit[fileNameSplit.length - 1] : ''
    const fileType = await getImageType(value, fileExtension, '.{0}')

    if (allowedFileTypes.every((t) => t !== fileType)) {
      return context.createError({
        message: ls.l('errors.InvalidFileType', value.name)
      })
    }
    return true
  }
}

/** Validate that files does not exceed provided size */
export const fileSize = (maxFileSizeInBytes: number): TestFunction => {
  return function (value, context) {
    let files: File[]
    if (Array.isArray(value)) {
      if (value.length && value.some((f) => !(f instanceof File))) {
        return context.createError({
          message: ls.l('errors.FieldNotValid', context.path)
        })
      }
      files = []
    } else if (value instanceof File) {
      files = [value]
    } else {
      return context.createError({
        message: ls.l('errors.FieldNotValid', context.path)
      })
    }

    let errorMessage = ''
    for (const file of files) {
      if (file.size > maxFileSizeInBytes) {
        errorMessage +=
          ls.l('errors.InvalidSizeForFile', file.name, formatDataSize(maxFileSizeInBytes)) + ';\n'
      }
    }

    if (errorMessage) {
      return context.createError({
        message: errorMessage
      })
    }
    return true
  }
}

export const uniqueFile = (existingFileHashes: IFileHashDto[]): TestFunction => {
  return async function (value, context) {
    if (
      value == null ||
      typeof value !== 'object' ||
      !('file' in value) ||
      !(value.file instanceof File)
    ) {
      return context.createError({
        message: ls.l('errors.FieldNotValid', context.path)
      })
    }

    const fileHash = await hashFile(value.file)
    if (existingFileHashes.some((h) => h === fileHash)) {
      return context.createError({
        message: ls.l('errors.FileAlreadySelected', value.file.name)
      })
    }

    return true
  }
}

export const requiredWhen = (predicate: Ref<boolean> | (() => boolean)): TestFunction => {
  return function (value, context) {
    const predicateValue = isRef(predicate) ? predicate.value : predicate()
    if (predicateValue && value == null) {
      return context.createError({
        message: ls.l('errors.RequiredField', context.path)
      })
    }
    return true
  }
}
