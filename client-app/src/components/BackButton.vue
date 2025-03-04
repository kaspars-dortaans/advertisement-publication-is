<template>
  <Button
    :severity="severity"
    :icon="icon"
    :label="label"
    as="RouterLink"
    :to="navigateTo"
    @click="() => navigation.setNextAsBackNavigation()"
  ></Button>
</template>

<script setup lang="ts">
import { AppNavigation } from '@/services/app-navigation'
import { onBeforeMount, ref } from 'vue'
import { type RouteLocationRaw } from 'vue-router'

const {
  defaultTo,
  icon = 'pi pi-arrow-left',
  label = '',
  severity = 'secondary'
} = defineProps<{
  defaultTo?: RouteLocationRaw
  icon?: string
  label?: string
  severity?: string
}>()

const navigation = AppNavigation.get()
const navigateTo = ref(defaultTo)

onBeforeMount(() => {
  if (navigation.hasPrevious()) {
    navigateTo.value = navigation.getPreviousFullPath
  }
})
</script>
