<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'manageAttributes' }"
    :title="isEdit ? l.navigation.editAttribute : l.navigation.createAttribute"
    :loading="loading || isSubmitting"
  >
    <form class="flex flex-col gap-4" @submit="submit">
      <FieldError :messages="formErrors" />

      <!-- Localized names -->
      <LocaleTextInput
        v-model="fields.localizedNames!.value"
        v-bind="fields.localizedNames!.attributes"
        :invalid="fields.localizedNames!.hasError"
        :localeList="ls.localeList.value"
        :label="l.form.attributeForm.title"
      />
      <FieldError :field="fields.localizedNames" />

      <!-- Value type -->
      <FloatLabel variant="on">
        <Select
          v-model="fields.valueType!.value"
          v-bind="fields.valueType!.attributes"
          :options="valueTypeOptions"
          :invalid="fields.valueType!.hasError"
          optionLabel="value"
          optionValue="key"
          id="value-type-input"
          fluid
        />
        <label for="value-type-input">{{ l.form.attributeForm.valueType }}</label>
      </FloatLabel>
      <FieldError :field="fields.valueType" />

      <!-- Attribute value list -->
      <template v-if="valueListType">
        <FloatLabel variant="on">
          <Select
            v-model="fields.attributeValueListId!.value"
            v-bind="fields.attributeValueListId!.attributes"
            :options="attributeValueListOptions"
            :invalid="fields.attributeValueListId!.hasError"
            optionLabel="value"
            optionValue="key"
            id="attribute-value-list-input"
            fluid
          />
          <label for="attribute-value-list-input">{{
            l.form.attributeForm.attributeValueList
          }}</label>
        </FloatLabel>
        <FieldError :field="fields.attributeValueListId" />
      </template>

      <!-- Filter type -->
      <FloatLabel variant="on">
        <Select
          v-model="fields.filterType!.value"
          v-bind="fields.filterType!.attributes"
          :options="filterTypeOptions"
          :invalid="fields.filterType!.hasError"
          optionLabel="value"
          optionValue="key"
          id="filter-type-input"
          fluid
        />
        <label for="filter-type-input">{{ l.form.attributeForm.filterType }}</label>
      </FloatLabel>
      <FieldError :field="fields.filterType" />

      <!-- valueValidationRegex -->
      <FloatLabel variant="on">
        <InputText
          v-model="fields.valueValidationRegex!.value"
          v-bind="fields.valueValidationRegex!.attributes"
          :invalid="fields.valueValidationRegex!.hasError"
          id="validation-regex-input"
          fluid
        />
        <label for="validation-regex-input">{{ l.form.attributeForm.valueValidationRegex }}</label>
      </FloatLabel>
      <FieldError :field="fields.valueValidationRegex" />

      <!-- sortable -->
      <div class="inline-flex gap-2 mt-4">
        <ToggleSwitch
          v-model="fields.sortable!.value"
          v-bind="fields.sortable!.attributes"
          :invalid="fields.sortable!.hasError"
          id="is-sortable-input"
        />
        <label for="is-sortable-input">{{ l.form.attributeForm.sortable }}</label>
      </div>
      <FieldError :field="fields.sortable" />

      <!-- searchable -->
      <div class="inline-flex gap-2 mt-4">
        <ToggleSwitch
          v-model="fields.searchable!.value"
          v-bind="fields.searchable!.attributes"
          :invalid="fields.searchable!.hasError"
          id="is-searchable-input"
        />
        <label for="is-searchable-input">{{ l.form.attributeForm.searchable }}</label>
      </div>
      <FieldError :field="fields.searchable" />

      <!-- showOnListItem -->
      <div class="inline-flex gap-2 mt-4">
        <ToggleSwitch
          v-model="fields.showOnListItem!.value"
          v-bind="fields.showOnListItem!.attributes"
          :invalid="fields.showOnListItem!.hasError"
          id="show-on-list-item-input"
        />
        <label for="show-on-list-item-input">{{ l.form.attributeForm.showOnListItem }}</label>
      </div>
      <FieldError :field="fields.showOnListItem" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.iconName!.value"
          v-bind="fields.iconName!.attributes"
          :invalid="fields.iconName!.hasError"
          id="icon-name-input"
          fluid
        />
        <label for="icon-name-input">{{ l.form.attributeForm.iconName }}</label>
      </FloatLabel>
      <FieldError :field="fields.iconName" />

      <Button :label="isEdit ? l.actions.save : l.actions.create" type="submit" class="mt-3" />
    </form>
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import FieldError from '@/components/form/FieldError.vue'
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import { useEnumOptionList } from '@/composables/enum-option-list'
import { FilterType } from '@/constants/api/FilterType'
import {
  AttributeClient,
  Int32StringKeyValuePair,
  PutAttributeRequest,
  StringStringKeyValuePair,
  ValueTypes
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { AttributeForm } from '@/types/forms/attribute-form'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { requiredWhen } from '@/validators/custom-validators'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, boolean, number, object, string } from 'yup'

const props = defineProps<{
  attributeId?: number
}>()

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const confirm = useConfirm()
const attributeService = getClient(AttributeClient)

//Reactive data
const loading = ref(0)
const isEdit = computed(() => typeof props.attributeId === 'number')
const valueTypeOptions = useEnumOptionList(ValueTypes, 'valueType')
const filterTypeOptions = useEnumOptionList(FilterType, 'filterType')
const attributeValueListOptions = ref<Int32StringKeyValuePair[]>([])
let valueListType = computed<boolean>(() => false)

//Forms and fields
const form = useForm<AttributeForm>({
  validationSchema: toTypedSchema(
    object({
      localizedNames: array().default([]).label('form.attributeForm.title'),
      valueType: string().required().label('form.attributeForm.valueType'),
      attributeValueListId: number()
        .nullable()
        .test(requiredWhen(() => valueListType.value))
        .label('form.attributeForm.attributeValueList'),
      filterType: string().required().label('form.attributeForm.filterType'),
      valueValidationRegex: string()
        .nullable()
        .default('')
        .label('form.attributeForm.valueValidationRegex'),
      sortable: boolean().default(true).label('form.attributeForm.sortable'),
      searchable: boolean().default(true).label('form.attributeForm.searchable'),
      showOnListItems: boolean().default(false).label('form.attributeForm.showOnListItem'),
      iconName: string().nullable().default('').label('form.attributeForm.iconName')
    })
  )
})
const fh = new FieldHelper(form)
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = fh
const { handleSubmit, values, isSubmitting, resetForm, validate } = form
defineMultipleFields([
  'valueType',
  'filterType',
  'searchable',
  'sortable',
  'showOnListItem',
  'attributeValueListId',
  'iconName',
  'valueValidationRegex',
  'localizedNames'
])
//Assign computed ref after values has been initialized
valueListType = computed<boolean>(() => values.valueType === ValueTypes.ValueListEntry)

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  await reloadData()
  validate({ mode: 'validated-only' })
})

//Methods
const reloadData = async () => {
  loading.value++

  if (isEdit.value) {
    const [formInfo] = await Promise.all([
      attributeService.getAttributeFormInfo(props.attributeId),
      loadLookups()
    ])
    resetForm({
      values: {
        id: formInfo.id,
        valueType: formInfo.valueType,
        filterType: formInfo.filterType,
        searchable: formInfo.searchable,
        sortable: formInfo.sortable,
        showOnListItem: formInfo.showOnListItem,
        attributeValueListId: formInfo.attributeValueListId,
        iconName: formInfo.iconName,
        valueValidationRegex: formInfo.valueValidationRegex,
        localizedNames: ls.localeList.value.map(
          (l) => formInfo.localizedNames.find((ln) => ln.key === l)?.value ?? ''
        )
      }
    })
  } else {
    await loadLookups()
  }

  loading.value--
}

const loadLookups = async () => {
  attributeValueListOptions.value = await attributeService.getAttributeValueListLookup()
}

const submit = handleSubmit(async () => {
  const localizedNames = values.localizedNames.map(
    (ln, i) =>
      new StringStringKeyValuePair({
        key: ls.localeList.value[i],
        value: ln
      })
  )
  try {
    const request = new PutAttributeRequest({
      id: values.id,
      valueType: values.valueType,
      filterType: values.filterType,
      searchable: values.searchable,
      sortable: values.sortable,
      showOnListItem: values.showOnListItem,
      attributeValueListId:
        values.valueType === ValueTypes.ValueListEntry ? values.attributeValueListId : undefined,
      iconName: values.iconName,
      valueValidationRegex: values.valueValidationRegex,
      localizedNames
    })

    if (isEdit.value) {
      await attributeService.editAttribute(request)
    } else {
      await attributeService.createAttribute(request)
    }

    formSubmitted.value = true
    push({ name: 'manageAttributes' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
