<template>
  <ResponsivePanel :defaultBackButtonRoute="{ name: 'home' }" :title="l.navigation.viewTermsOfUse">
    <p>{{ l.termsOfUse }}</p>
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import { LocaleService } from '@/services/locale-service'

//Services
const l = LocaleService.currentLocale
</script>
