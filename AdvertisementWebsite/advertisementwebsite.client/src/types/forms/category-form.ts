import type { Int32StringKeyValuePair, PutCategoryRequest } from '@/services/api-client'

export type CategoryForm = Omit<PutCategoryRequest, 'localizedNames' | 'parentCategoryId'> & {
  localizedNames: string[]
  parentCategory?: Int32StringKeyValuePair
}
