<template>
  <Button
    :severity="severity"
    :icon="noIcon ? '' : icon"
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

const props = withDefaults(
  defineProps<{
    defaultTo?: RouteLocationRaw
    icon?: string
    noIcon?: boolean
    label?: string
    severity?: string
  }>(),
  {
    icon: 'pi pi-arrow-left',
    severity: 'secondary'
  }
)

const navigation = AppNavigation.get()
const navigateTo = ref(props.defaultTo)

onBeforeMount(() => {
  if (navigation.hasPrevious()) {
    navigateTo.value = navigation.getPreviousFullPath
  }
})
</script>
