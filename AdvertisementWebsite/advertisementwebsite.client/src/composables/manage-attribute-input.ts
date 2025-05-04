import {
  CategoryClient,
  CategoryFormInfo,
  CategoryItem,
  ValueTypes,
  type AttributeFormInfo,
  type AttributeValueListItem
} from '@/services/api-client'
import { FieldHelper } from '@/utils/field-helper'
import { matchArrayElement } from '@/validators/custom-validators'
import { type FormContext, type Path, type PathValue } from 'vee-validate'
import { ref, type Ref } from 'vue'
import { number, Schema, string, type AnyObject } from 'yup'

type formType = { attributeValues: (string | number | undefined)[]; categoryId: number }

export const useValidateAttributeInput = (attributeInfo: Ref<AttributeFormInfo[]>) => {
  const addAttributeValidationSchema = (schemaObject: AnyObject) => {
    //Add attribute validation
    for (let i = attributeInfo.value.length - 1; i >= 0; i--) {
      const attribute = attributeInfo.value[i]
      let validation: Schema | undefined = undefined
      switch (attribute.attributeValueType) {
        case ValueTypes.Text:
          validation = string()
          break
        case ValueTypes.Integer:
          validation = number().integer()
          break
        case ValueTypes.Decimal:
          validation = number()
          break
        case ValueTypes.ValueListEntry:
          validation = number()
          break
      }

      if (validation && attribute.valueValidationRegex) {
        const regexp = new RegExp(attribute.valueValidationRegex)
        validation = validation.test(matchArrayElement(regexp, attribute.name))
        schemaObject[`attributeValues[${i}]`] = validation
      }
    }
  }

  return { addAttributeValidationSchema }
}

export const useManageAttributeInput = <F extends formType>(
  form: FormContext<F>,
  fieldHelper: FieldHelper<F>,
  categoryList: Ref<CategoryItem[]>,
  attributeInfo: Ref<AttributeFormInfo[]>,
  categoryService: CategoryClient
) => {
  const { setFieldValue, validateField, values } = form

  const loadingAttributes = ref(false)
  const attributeValueLists = ref<AttributeValueListItem[]>([])
  const loading = ref<number>(0)

  const loadCategoryList = async () => {
    loading.value += 1
    categoryList.value = await categoryService.getCategories()
    loading.value -= 1
  }

  const loadCategoryInfo = async (categoryId: number) => {
    const loadFlag = attributeInfo.value.length ? loadingAttributes : loading
    loadFlag.value = true
    setFieldValue('attributeValues' as Path<F>, [] as PathValue<F, Path<F>>)
    const result = await categoryService.getCategoryFormInfo(categoryId)
    setCategoryInfo(result)
    loadFlag.value = false
  }

  const handleSelectedCategory = async (newValue: number) => {
    setFieldValue('categoryId' as Path<F>, newValue as PathValue<F, Path<F>>)
    const categoryIdValidationResult = await validateField('categoryId' as Path<F>)

    if (categoryIdValidationResult.valid) {
      loadCategoryInfo(values.categoryId)
    } else {
      attributeInfo.value = []
      attributeValueLists.value = []
    }
  }

  const setCategoryInfo = (categoryInfo?: CategoryFormInfo) => {
    if (categoryInfo?.attributeInfo) {
      ensureAttributesHaveModels(categoryInfo.attributeInfo)
    }

    attributeValueLists.value = categoryInfo?.attributeValueLists ?? []
    attributeInfo.value = categoryInfo?.attributeInfo ?? []
  }

  const ensureAttributesHaveModels = (attributeInfo: AttributeFormInfo[]) => {
    const existingAttributeModelCount = Object.keys(fieldHelper.fields).filter((k) =>
      k.startsWith('attributeValues[')
    ).length

    if (attributeInfo.length && existingAttributeModelCount < attributeInfo.length) {
      const fieldNames: `attributeValues.${number}`[] = []
      for (let i = existingAttributeModelCount; i < attributeInfo.length; i++) {
        fieldNames.push(`attributeValues.${i}`)
      }
      fieldHelper.defineMultipleFields(fieldNames as Path<F>[])
    }
  }

  return {
    loading,
    loadingAttributes,
    attributeInfo,
    attributeValueLists,
    setCategoryInfo,
    loadCategoryInfo,
    loadCategoryList,
    handleSelectedCategory
  }
}
