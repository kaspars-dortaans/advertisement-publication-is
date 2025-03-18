<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
      <Panel>
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'viewAdvertisement' }" />
            <h3 class="page-title">{{ l.navigation.editAdvertisement }}</h3>
          </div>
        </template>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import BackButton from '@/components/BackButton.vue'
import BlockWithSpinner from '@/components/Common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import { LocaleService } from '@/services/locale-service'
import { ref } from 'vue'

const l = LocaleService.currentLocale

const loading = ref(false)

//TODO: Complete
</script>
