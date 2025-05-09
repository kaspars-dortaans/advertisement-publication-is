<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading || isSubmitting" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1 lg:min-w-96">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAttributeValueLists' }" />
            <h4 class="page-title">
              {{
                isEdit ? l.navigation.editAttributeValueList : l.navigation.createAttributeValueList
              }}
            </h4>
          </div>
        </template>

        <form class="flex flex-col" @submit="submit">
          <FieldError :messages="formErrors" />

          <LocaleTextInput
            v-model="fields.title!.value"
            v-bind="fields.title!.attributes"
            :invalid="fields.title!.hasError"
            :localeList="ls.localeList.value"
            :label="l.form.attributeValueListForm.title"
          />
          <FieldError :field="fields.title" />

          <Divider />

          <AttributeValueListEntryInput
            v-model="fields.entries!.value"
            v-bind="fields.entries!.attributes"
            :invalid="fields.entries!.hasError"
          />
          <FieldError :field="fields.entries" />

          <Button :label="isEdit ? l.actions.save : l.actions.create" type="submit" class="mt-3" />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import AttributeValueListEntryInput from '@/components/attribute-input/AttributeValueListEntryInput.vue'
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import {
  AttributeClient,
  PutAttributeValueListRequest,
  StringStringKeyValuePair
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, object } from 'yup'
import { type AttributeValueListForm } from '../../types/forms/attribute-value-list-form'

const props = defineProps<{
  valueListId?: number
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
const isEdit = computed(() => typeof props.valueListId === 'number')

//Forms and fields
const form = useForm<AttributeValueListForm>({
  validationSchema: toTypedSchema(
    object({
      title: array().required().default([]).label('form.manageAttributeValueLists.title'),
      entries: array().required().default([]).label('form.manageAttributeValueLists.entryCount')
    })
  )
})
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, resetForm, validate } = form
defineMultipleFields(['title', 'entries'])

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
  if (isEdit.value) {
    loading.value++
    const formInfo = await attributeService.getAttributeValueListFormInfo(props.valueListId)
    resetForm({
      values: {
        title: ls.localeList.value.map(
          (l) => formInfo.localizedNames?.find((ln) => ln.key === l)?.value ?? ''
        ),
        entries: formInfo.entries ?? []
      }
    })
    loading.value--
  }
}

const submit = handleSubmit(async () => {
  try {
    const request = new PutAttributeValueListRequest({
      id: props.valueListId,
      localizedNames: ls.localeList.value.map(
        (l, i) =>
          new StringStringKeyValuePair({
            key: l,
            value: values.title[i]
          })
      ),
      entries: values.entries
    })

    if (isEdit.value) {
      await attributeService.editAttributeValueList(request)
    } else {
      await attributeService.createAttributeValueList(request)
    }

    formSubmitted.value = true
    push({ name: 'manageAttributeValueLists' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
