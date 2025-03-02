<template>
  <Button :severity="severity" :icon="icon" as="RouterLink" :to="navigateTo"></Button>
</template>

<script setup lang="ts">
import { onBeforeMount, ref } from 'vue'
import { useRoute, type RouteLocationRaw } from 'vue-router'

const {
  to,
  icon = 'pi pi-arrow-left',
  severity = 'secondary'
} = defineProps<{ to?: RouteLocationRaw; icon?: string; severity?: string }>()

const { matched } = useRoute()

const navigateTo = ref(to)
onBeforeMount(() => {
  if (!navigateTo.value) {
    if (matched.length > 1) {
      //If current route is child route navigate to parent route
      navigateTo.value = matched[matched.length - 2]
    } else {
      //Else navigate to home page
      navigateTo.value = { name: 'home' }
    }
  }
})
</script>
