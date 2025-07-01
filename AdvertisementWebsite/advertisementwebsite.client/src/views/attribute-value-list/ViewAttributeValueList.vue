<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'manageAttributeValueLists' }"
    :title="l.navigation.viewAttributeValueList"
    :loading="loading"
  >
    <template #titlePanelButtons>
      <Button
        v-if="isAllowedToEdit"
        icon="pi pi-pencil"
        severity="secondary"
        :label="l.actions.edit"
        as="RouterLink"
        :to="{ name: 'editAttributeValueList', params: { valueListId } }"
      />
    </template>

    <div class="flex flex-col">
      <LocaleTextInput
        v-model="valueList.title"
        :localeList="ls.localeList.value"
        :label="l.form.attributeValueListForm.title"
        disabled
      />

      <Divider />

      <AttributeValueListEntryInput v-model="valueList.entries" disabled />
    </div>
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import AttributeValueListEntryInput from '@/components/attribute-input/AttributeValueListEntryInput.vue'
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import { AttributeClient } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onBeforeMount, ref, watch } from 'vue'
import { type AttributeValueListForm } from '../../types/forms/attribute-value-list-form'
import { AuthService } from '@/services/auth-service'
import { Permissions } from '@/constants/api/Permissions'
import { computed } from 'vue'
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'

const props = defineProps<{
  valueListId?: number
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const attributeService = getClient(AttributeClient)

//Reactive data
const loading = ref(0)
const valueList = ref<AttributeValueListForm>({
  title: [],
  entries: []
})
const isAllowedToEdit = computed(() =>
  AuthService.hasPermission(Permissions.EditAttributeValueList)
)

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  await reloadData()
})

//Methods
const reloadData = async () => {
  loading.value++
  const formInfo = await attributeService.getAttributeValueListFormInfo(props.valueListId)
  valueList.value = {
    title: ls.localeList.value.map(
      (l) => formInfo.localizedNames?.find((ln) => ln.key === l)?.value ?? ''
    ),
    entries: formInfo.entries ?? []
  }
  loading.value--
}
</script>
