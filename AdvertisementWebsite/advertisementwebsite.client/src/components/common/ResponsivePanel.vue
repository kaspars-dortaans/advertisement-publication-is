<template>
  <BlockWithSpinner :loading="isLoading" class="responsive-flex-child">
    <Panel
      class="flex-1 rounded-none lg:rounded-md lg:min-w-96 h-0 flex flex-col"
      :class="panelClass"
      :pt="{
        contentcontainer: 'flex-1 min-h-0 flex flex-col',
        content: 'flex-1 min-h-0 flex flex-col',
        header: () => ({ class: { hidden: !showHeader } })
      }"
    >
      <template #header v-if="showHeader">
        <div class="panel-title-container">
          <BackButton v-if="defaultBackButtonRoute" :defaultTo="defaultBackButtonRoute" />
          <h4 v-if="title" class="page-title">{{ title }}</h4>
          <slot name="titlePanelButtons"></slot>
        </div>
      </template>

      <slot></slot>

      <template #footer v-if="slots.footer">
        <slot name="footer"></slot>
      </template>
    </Panel>
  </BlockWithSpinner>
</template>

<script lang="ts" setup>
import { computed, useSlots } from 'vue'
import BackButton from './BackButton.vue'
import BlockWithSpinner from './BlockWithSpinner.vue'
import type { RouteLocationAsPathGeneric, RouteLocationAsRelativeGeneric } from 'vue-router'

const props = defineProps<{
  defaultBackButtonRoute?: string | RouteLocationAsPathGeneric | RouteLocationAsRelativeGeneric
  title?: string
  loading?: boolean | number
  panelClass?: string | undefined
}>()

const slots = useSlots()

const showHeader = computed(
  () => props.defaultBackButtonRoute || props.title || slots.titlePanelButtons
)

const isLoading = computed(() =>
  typeof props.loading === 'number' ? props.loading > 0 : !!props.loading
)
</script>
