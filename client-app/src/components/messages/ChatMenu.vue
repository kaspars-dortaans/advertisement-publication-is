<template>
  <Menu :model="menuItems" class="overflow-y-auto h-full">
    <template #item="{ item }">
      <div class="flex flex-row gap-2 items-center p-2">
        <template v-if="item.chat">
          <img :src="item.chat.thumbnailImageUrl" class="w-24 h-24 object-cover rounded-md" />
          <div class="space-3">
            <div class="inline-flex gap-2 items-baseline">
              <h4 class="font-semibold line-clamp">{{ item.chat.title }}</h4>
              <Badge
                v-if="item.chat.unreadMessageCount && item.chat.unreadMessageCount > 0"
                :value="item.chat.unreadMessageCount"
                class="min-w-6 min-h-6 rounded-xl"
              />
            </div>
            <p class="line-clamp-2 break-all">{{ item.chat.lastMessage }}</p>
          </div>
        </template>
        <span v-else class="mx-auto">{{ item.label }}</span>
      </div>
    </template>
  </Menu>
</template>

<script lang="ts" setup>
import type { IChatMenuItem } from '@/types/messages/chat-menu-item'

const { menuItems } = defineProps<{
  menuItems: IChatMenuItem[]
}>()
</script>
