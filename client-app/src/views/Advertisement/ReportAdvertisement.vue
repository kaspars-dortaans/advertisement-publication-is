<template>
  <div class="h-full bg-primary flex items-center justify-center">
    <Panel class="max-w-96 w-96">
      <template #header>
        <h3 class="text-2xl">{{ l.advertisements.reportRuleViolation }}</h3>
      </template>
      <form class="flex flex-col gap-4 min-h-80">
        <label for="report-text-area">{{ l.advertisements.description }}</label>
        <Textarea
          v-model="fields.description!.value"
          v-bind="fields.description?.attributes"
          :invalid="fields.description?.hasError"
          class="flex-1"
          id="report-text-area"
        ></Textarea>
        <FieldError :field="fields.description" />

        <div class="flex flex-row gap-4 justify-center">
          <BackButton
            icon=""
            :label="l.actions.cancel"
            :defaultTo="{ name: 'viewAdvertisement' }"
          ></BackButton>
          <Button
            type="button"
            :label="l.actions.report"
            :loading="isSubmitting"
            @click="report"
          ></Button>
        </div>
      </form>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import BackButton from '@/components/BackButton.vue'
import FieldError from '@/components/Form/FieldError.vue'
import { AdvertisementClient, ReportAdvertisementRequest } from '@/services/api-client'
import { AppNavigation } from '@/services/app-navigation'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { object, string } from 'yup'

const { push } = useRouter()
const { id: advertisementId } = defineProps<{ id: number }>()

const l = LocaleService.currentLocale
const navigation = AppNavigation.get()
const _advertisementService = getClient(AdvertisementClient)

const form = useForm({
  validationSchema: toTypedSchema(
    object({
      description: string().required()
    })
  )
})
const { values, handleSubmit, isSubmitting } = form
const { defineField, handleErrors, fields } = new FieldHelper(form)
defineField('description')

onMounted(() => {
  if (typeof advertisementId !== 'number' || isNaN(advertisementId)) {
    push({ name: 'NotFound' })
  }
})

const report = handleSubmit(async () => {
  try {
    await _advertisementService.reportAdvertisement(
      new ReportAdvertisementRequest({
        description: values.description,
        reportedAdvertisementId: advertisementId
      })
    )
    if (navigation.hasPrevious()) {
      push(navigation.getPreviousFullPath)
    } else {
      push({ name: 'viewAdvertisement' })
    }
  } catch (e) {
    handleErrors(e)
  }
})
</script>
