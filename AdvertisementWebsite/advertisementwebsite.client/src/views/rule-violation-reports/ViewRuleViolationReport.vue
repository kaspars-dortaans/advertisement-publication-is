<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading || isSubmitting" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="flex-1 rounded-none lg:rounded-md lg:min-w-[720px]">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageRuleViolationReports' }" />
            <h4 class="page-title">
              {{ l.navigation.viewRuleViolationReport }}
            </h4>
          </div>
        </template>

        <dl class="grid grid-cols-[auto_auto] gap-2">
          <dt>{{ l.form.resolveRuleViolationReport.status }}</dt>
          <dl>
            {{ l.manageRuleViolationReports[report.isResolved ? 'resolved' : 'unresolved'] }}
          </dl>

          <dt>{{ l.manageRuleViolationReports.description }}</dt>
          <dl>{{ report.description }}</dl>

          <template v-if="report.reporterId">
            <dt>{{ l.manageRuleViolationReports.reporter }}</dt>
            <dl>
              <RouterLink
                :to="{
                  name: isAllowedToViewAllUsers ? 'viewUser' : 'viewUserProfile',
                  params: { userId: report.reporterId }
                }"
                >{{ report.reporterUsername }}</RouterLink
              >
            </dl>
          </template>

          <template v-if="report.advertisementId">
            <dt>{{ l.manageRuleViolationReports.reportedAdvertisement }}</dt>
            <dl>
              <RouterLink
                :to="{ name: 'viewAdvertisement', params: { id: report.advertisementId } }"
                >{{ report.advertisementTitle }}</RouterLink
              >
            </dl>
          </template>

          <template v-if="report.advertisementOwnerId">
            <dt>{{ l.manageRuleViolationReports.advertisementOwner }}</dt>
            <dl>
              <RouterLink
                :to="{
                  name: isAllowedToViewAllUsers ? 'viewUser' : 'viewUserProfile',
                  params: { userId: report.advertisementOwnerId }
                }"
              >
                {{ report.advertisementOwnerUsername }}
              </RouterLink>
            </dl>
          </template>

          <dt>{{ l.manageRuleViolationReports.reportDate }}</dt>
          <dl>{{ dateFormat.format(report.reportDate) }}</dl>

          <template v-if="typeof report.isTrue === 'boolean' && !showResolveForm">
            <dt>{{ l.form.resolveRuleViolationReport.isTrue }}</dt>
            <dl>{{ l[(report.isTrue ?? false).toString()] }}</dl>

            <dt>{{ l.form.resolveRuleViolationReport.resolutionDescription }}</dt>
            <dl>{{ report.resolutionDescription }}</dl>
          </template>
        </dl>

        <Button
          v-if="!showResolveForm && isAllowedToResolve"
          class="w-full mt-5 lg:w-fit lg:mx-auto"
          @click="showResolveForm = true"
        >
          {{ report.isResolved ? l.actions.edit : l.actions.resolve }}
        </Button>

        <form v-if="showResolveForm" class="flex flex-col gap-4" @submit="submit">
          <FieldError :messages="formErrors" />

          <Divider />

          <!-- isTrue -->
          <div class="inline-flex gap-2">
            <ToggleSwitch
              v-model="fields.isTrue!.value"
              v-bind="fields.isTrue!.attributes"
              :invalid="fields.isTrue!.hasError"
              id="is-true-input"
            />
            <label for="is-true-input">{{ l.form.resolveRuleViolationReport.isTrue }}</label>
          </div>

          <!-- resolutionDescription -->
          <FloatLabel variant="on">
            <Textarea
              v-model="fields.resolutionDescription!.value"
              v-bind="fields.resolutionDescription!.attributes"
              :invalid="fields.resolutionDescription!.hasError"
              id="resolution-description-input"
              fluid
              autoResize
            />
            <label for="resolution-description-input">{{
              l.form.resolveRuleViolationReport.resolutionDescription
            }}</label>
          </FloatLabel>

          <div class="flex flex-row flex-wrap gap-2 justify-center mt-3">
            <Button
              :label="l.actions.cancel"
              type="button"
              severity="secondary"
              class="flex-grow basis-auto lg:flex-none"
              @click="cancelForm"
            />
            <Button
              :label="l.actions.save"
              type="submit"
              class="flex-grow basis-auto lg:flex-none"
            />
          </div>
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import {
  ResolveRuleViolationReportRequest,
  RuleViolationReportClient,
  RuleViolationReportListItem
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { AuthService } from '@/services/auth-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { Textarea, useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch, computed } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'
import { boolean, object, string } from 'yup'
import { Permissions } from '@/constants/api/Permissions'

const props = defineProps<{
  id: number
}>()

//Route
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const confirm = useConfirm()
const ruleViolationReportService = getClient(RuleViolationReportClient)

//Reactive data
const loading = ref(0)
const report = ref(new RuleViolationReportListItem({}))
const isAllowedToResolve = computed(() =>
  AuthService.hasPermission(Permissions.ResolveRuleViolationReport)
)
const showResolveForm = ref(false)
const isAllowedToViewAllUsers = computed(() => AuthService.hasPermission(Permissions.ViewAllUsers))
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

//Forms and fields
const form = useForm<ResolveRuleViolationReportRequest>({
  validationSchema: toTypedSchema(
    object({
      isTrue: boolean().default(false).required().label('manageRuleViolationReports.isTrue'),
      resolutionDescription: string()
        .default('')
        .required()
        .label('manageRuleViolationReports.resolutionDescription')
    })
  )
})
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, resetForm, validate } = form
defineMultipleFields(['isTrue', 'resolutionDescription'])

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  validate({ mode: 'validated-only' })
})

//Methods
const reloadData = async () => {
  loading.value++
  report.value = await ruleViolationReportService.getReport(props.id)
  resetForm({
    values: {
      isTrue: report.value.isTrue,
      resolutionDescription: report.value.resolutionDescription
    }
  })
  loading.value--
}

const submit = handleSubmit(async () => {
  try {
    await ruleViolationReportService.resolveReport(
      new ResolveRuleViolationReportRequest({
        id: props.id,
        isTrue: values.isTrue,
        resolutionDescription: values.resolutionDescription
      })
    )
    showResolveForm.value = false
    await reloadData()
  } catch (e) {
    handleErrors(e)
  }
})

const cancelForm = () => {
  showResolveForm.value = false
  resetForm({
    values: {
      isTrue: report.value.isTrue,
      resolutionDescription: report.value.resolutionDescription
    }
  })
}
</script>
