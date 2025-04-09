<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
      <Panel class="rounded-none flex-1 lg:flex-none lg:rounded-md">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAdvertisements' }" />
            <h3 class="page-title">{{ l.form.extendAdvertisements.pageTitle }}</h3>
          </div>
        </template>
        <form class="flex gap-3 flex-col" @submit="submit">
          <Panel v-if="advertisements?.length" toggleable>
            <template #header>
              <p>
                {{ ls.l('form.extendAdvertisements.extendedList', advertisementIds.length) }}
              </p>
            </template>
            <ul class="list-disc pl-4">
              <li v-for="advertisement in advertisements" :key="advertisement.id">
                {{ advertisement.title }}
              </li>
            </ul>
          </Panel>
          <FloatLabel variant="on">
            <Select
              v-model="fields.extendTime!.model"
              v-bind="fields.extendTime?.attributes"
              :options="timeOptions"
              :invalid="fields.extendTime?.hasError"
              optionLabel="name"
              optionValue="value"
              id="time-input"
              fluid
            />
            <label for="time-input">{{ l.form.extendAdvertisements.timePeriod }}</label>
          </FloatLabel>
          <FieldError :field="fields.extendTime" />

          <div class="flex flex-row gap-2 justify-center">
            <BackButton
              :label="l.actions.cancel"
              icon=""
              :default-to="{ name: 'manageAdvertisements' }"
            />
            <Button type="submit" :loading="isSubmitting">{{ l.actions.extend }}</Button>
          </div>
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import {
  AdvertisementClient,
  AdvertisementListItem,
  AdvertisementQuery,
  ExtendAdvertisementRequest
} from '@/services/api-client'
import { AppNavigation } from '@/services/app-navigation'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref } from 'vue'
import { useRouter } from 'vue-router'
import { number, object } from 'yup'

//Route
const { push } = useRouter()

//Props
const { advertisementIds } = defineProps<{
  advertisementIds: number[]
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const navigation = AppNavigation.get()

//Reactive data
const timeOptions = computed(() => {
  return createAdvertisementPostTimeSpanOptions(ls)
})
const loading = ref(false)
const advertisements = ref<AdvertisementListItem[]>()

//Forms and fields
const form = useForm<ExtendAdvertisementRequest>({
  validationSchema: toTypedSchema(
    object({
      extendTime: object({
        days: number(),
        weeks: number(),
        months: number()
      })
        .default(undefined)
        .required()
    })
  )
})
const { fields, handleErrors, defineField } = new FieldHelper(form)
const { handleSubmit, isSubmitting, values } = form
defineField('extendTime')

//Hooks
onBeforeMount(async () => {
  loading.value = true

  const result = await advertisementService.getAdvertisements(
    new AdvertisementQuery({
      advertisementIds: advertisementIds,
      order: [],
      columns: [],
      attributeSearch: [],
      attributeOrder: []
    })
  )
  advertisements.value = result.data

  loading.value = false
})

//Methods
const submit = handleSubmit(async () => {
  try {
    await advertisementService.extendAdvertisements(
      new ExtendAdvertisementRequest({
        advertisementIds: advertisementIds,
        extendTime: values.extendTime
      })
    )
    push(
      navigation.hasPrevious() ? navigation.getPreviousFullPath : { name: 'manageAdvertisements' }
    )
  } catch (e) {
    handleErrors(e)
  }
})
</script>
