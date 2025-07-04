<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'viewAdvertisement' }"
    :title="l.advertisements.reportRuleViolation"
  >
    <form class="flex flex-col gap-4 min-h-80" @submit="report">
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
          :label="l.actions.cancel"
          :defaultTo="{ name: 'viewAdvertisement' }"
          noIcon
        ></BackButton>
        <Button type="submit" :label="l.actions.report" :loading="isSubmitting"></Button>
      </div>
    </form>
  </ResponsivePanel>
</template>

<script setup lang="ts">
import BackButton from '@/components/common/BackButton.vue'
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import FieldError from '@/components/form/FieldError.vue'
import { ReportAdvertisementRequest, RuleViolationReportClient } from '@/services/api-client'
import { AppNavigation } from '@/services/app-navigation'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onMounted, ref } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { object, string } from 'yup'

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
const confirm = useConfirm()
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Props
const props = defineProps<{ id: number }>()

//Services
const l = LocaleService.currentLocale
const navigation = AppNavigation.get()
const ruleViolationReportService = getClient(RuleViolationReportClient)

//Form and fields
const form = useForm({
  validationSchema: toTypedSchema(
    object({
      description: string().required().default('')
    })
  )
})
const { values, handleSubmit, isSubmitting } = form
const { fields, valuesChanged, defineField, handleErrors } = new FieldHelper(form)
defineField('description')

//Hooks
onMounted(() => {
  if (typeof props.id !== 'number' || isNaN(props.id)) {
    push({ name: 'NotFound' })
  }
})

//Methods
const report = handleSubmit(async () => {
  try {
    await ruleViolationReportService.reportAdvertisement(
      new ReportAdvertisementRequest({
        description: values.description!,
        reportedAdvertisementId: props.id
      })
    )
    formSubmitted.value = true
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
