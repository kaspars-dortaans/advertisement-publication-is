<template>
  <NavBar></NavBar>
  <ResponsiveLayout :containerClass="route.meta.containerClass as string">
    <RouterView />
  </ResponsiveLayout>

  <ConfirmDialog />
  <Toast position="top-right" group="tr" />
</template>

<script setup lang="ts">
import NavBar from '@/components/NavBar.vue'
import { ConfirmDialog } from 'primevue'
import Toast from 'primevue/toast'
import ResponsiveLayout from './components/common/ResponsiveLayout.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>
