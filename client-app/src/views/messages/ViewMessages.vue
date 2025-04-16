<template>
  <ResponsiveLayout containerClass="h-0">
    <div class="h-full flex-1 max-w-6xl">
      <div
        class="max-h-full min-h-full lg:min-h-96 w-full bg-surface-0 p-[1.125rem] gap-[1.125rem] rounded-none lg:rounded-md flex flex-col flex-nowrap"
      >
        <h3 class="page-title">{{ l.navigation.messages }}</h3>

        <div class="flex flex-1 h-40">
          <BlockWithSpinner
            :loading="loadingChats"
            class="overflow-y-auto overflow-x-visible h-full"
          >
            <Tabs :value="tabOpened">
              <TabList>
                <Tab :value="0">
                  {{ l.messages.userAdvertisementChats }}
                  <Badge
                    v-if="userAdvertisementUnreadMessageCount > 0"
                    :value="userAdvertisementUnreadMessageCount"
                    class="min-w-6 min-h-6 rounded-xl"
                  />
                </Tab>
                <Tab :value="1">
                  {{ l.messages.otherUserAdvertisementChats }}
                  <Badge
                    v-if="otherAdvertisementUnreadMessageCount > 0"
                    :value="otherAdvertisementUnreadMessageCount"
                    class="min-w-6 min-h-6 rounded-xl"
                /></Tab>
              </TabList>
              <TabPanels :pt="{ root: 'pl-0 pb-0 pr-0' }">
                <TabPanel :value="0">
                  <ChatMenu :menuItems="userAdvertisementChats" />
                </TabPanel>
                <TabPanel :value="1">
                  <ChatMenu :menuItems="otherUserAdvertisementChats" />
                </TabPanel>
              </TabPanels>
            </Tabs>
          </BlockWithSpinner>

          <Divider layout="vertical" />

          <BlockWithSpinner
            :loading="loadingMessages"
            class="max-h-full flex-1 flex flex-col flex-nowrap gap-2"
          >
            <ChatMessages
              v-model:loading="loadingMessages"
              :currentChatId="currentChat?.id"
              :currentAdvertisementId="currentChat?.advertisementId"
              :isNewChat="isNewChat"
              ref="chatMessages"
            />
            <form
              class="flex-none flex flex-row flex-wrap items-end gap-2 mt-auto"
              @submit="sendMessage"
            >
              <Button class="min-w-10" icon="pi pi-paperclip" rounded @click="selectAttachments" />
              <div class="flex-1">
                <Textarea
                  v-model="fields.text!.model.value"
                  v-bind="fields.text?.attributes"
                  :valid="fields.text?.hasError"
                  :rows="1"
                  fluid
                  autoResize
                ></Textarea>
                <FieldError :field="fields.text" />
              </div>
              <Button
                :disabled="!currentChat && !isNewChat"
                class="min-w-10"
                icon="pi pi-arrow-right"
                type="submit"
                :loading="sendingMessage"
                rounded
              />
              <AttachmentUpload
                v-model="fields.attachments!.model.value"
                v-bind="fields.attachments?.attributes"
                :invalid="fields.attachments?.hasError"
                ref="fileInput"
                class="w-full"
              />
              <FieldError :field="fields.attachments" />
            </form>
          </BlockWithSpinner>
        </div>
      </div>
    </div>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import AttachmentUpload from '@/components/messages/AttachmentUpload.vue'
import ChatMenu from '@/components/messages/ChatMenu.vue'
import ChatMessages from '@/components/messages/ChatMessages.vue'
import { AttachmentsConstants } from '@/constants/api/AttachmentsConstants'
import { NewMessageTimeout } from '@/constants/message'
import {
  ChatListItemDto,
  MessageClient,
  SendMessageRequest,
  type FileParameter,
  type IChatListItemDto,
  type IMessageItemDto,
  type ISendMessageRequest
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { MessageHub } from '@/services/message-hub'
import type { IChatMenuItem } from '@/types/messages/chat-menu-item'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { fileSize } from '@/validators/custom-validators'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { computed, inject, onBeforeMount, onBeforeUnmount, ref, useTemplateRef } from 'vue'
import { useRouter } from 'vue-router'
import { array, object, string } from 'yup'

//Props
const { chatId, newChatToAdvertisementId, newChatToUserId } = defineProps<{
  chatId?: number
  newChatToUserId?: number
  newChatToAdvertisementId?: number
}>()

//Services
const l = LocaleService.currentLocale
const messageHub = inject<MessageHub>('messageHub')!
const messageService = getClient(MessageClient)
const { replace } = useRouter()

//Refs
const fileInputEl = useTemplateRef('fileInput')
const chatMessageEl = useTemplateRef('chatMessages')

//Reactive data
const loadingChats = ref(false)
const loadingMessages = ref(false)
const sendingMessage = ref(false)

const chatMenuItems = ref<IChatMenuItem[]>([])
const otherUserAdvertisementChats = computed(() => {
  const items = chatMenuItems.value.filter(
    (i) => i.chat.advertisementOwnerId !== currentUserId.value
  )
  if (!items.length) {
    items.push({
      label: l.value.messages.noChatsYet,
      disabled: true
    } as IChatMenuItem)
  }
  return items
})
const userAdvertisementUnreadMessageCount = computed(() =>
  userAdvertisementChats.value.reduce((result, i) => result + (i.chat?.unreadMessageCount ?? 0), 0)
)
const userAdvertisementChats = computed(() => {
  const items = chatMenuItems.value.filter(
    (i) => i.chat.advertisementOwnerId === currentUserId.value
  )
  if (!items.length) {
    items.push({
      label: l.value.messages.noChatsYet,
      disabled: true
    } as IChatMenuItem)
  }
  return items
})
const otherAdvertisementUnreadMessageCount = computed(() =>
  otherUserAdvertisementChats.value.reduce(
    (result, i) => result + (i.chat?.unreadMessageCount ?? 0),
    0
  )
)
const tabOpened = ref<0 | 1>(0)

const currentUserId = computed(() => AuthService.profileInfo.value?.id)
const currentChat = ref<ChatListItemDto | undefined>()
const isNewChat = computed(() => newChatToUserId != null)

const unsubscribeNewChat = ref<(() => void) | undefined>()
const unsubscribeNewMessage = ref<(() => void) | undefined>()
const unsubscribeMarkMessageAsRead = ref<(() => void) | undefined>()
const unsubscribeOnReconnect = ref<() => void | undefined>()

//Forms and fields
const form = useForm<SendMessageRequest>({
  validationSchema: toTypedSchema(
    object({
      text: string().required().default(''),
      attachments: array()
        .default([])
        .max(AttachmentsConstants.MaxAttachmentCount)
        .test(fileSize(AttachmentsConstants.MaxAttachmentSizeInBytes))
    })
  )
})
const { handleSubmit, values, resetForm } = form
const { fields, defineMultipleFields, handleErrors } = new FieldHelper(form)
defineMultipleFields(['text', 'attachments'])

//Hooks
onBeforeMount(async () => {
  const unsubscribeNewChatPromise = messageHub.subscribeNewChat(handleNewChat)
  unsubscribeNewChat.value = await unsubscribeNewChatPromise
  unsubscribeNewMessage.value = await messageHub.subscribeNewMessages(handleNewMessage)
  unsubscribeMarkMessageAsRead.value =
    await messageHub.subscribeMarkMessageAsRead(handleMessageRead)
  unsubscribeOnReconnect.value = messageHub.subscribeToReconnect(loadChats)
  await loadChats()
})

onBeforeUnmount(async () => {
  unsubscribeNewChat.value?.()
  unsubscribeNewMessage.value?.()
  unsubscribeMarkMessageAsRead.value?.()
  unsubscribeOnReconnect.value?.()
})

//Methods
/**
 * Select chat
 * @param selectedChat
 */
const selectChat = (selectedChat: ChatListItemDto) => {
  for (const item of chatMenuItems.value) {
    item.class = selectedChat.id === item.chat.id ? 'active' : ''
  }
  tabOpened.value = selectedChat.advertisementOwnerId == currentUserId.value ? 0 : 1
  currentChat.value = selectedChat
  replace({ name: 'viewMessages', params: { chatId: currentChat.value.id! } })
}

/**
 * Call Api to load chats
 */
const loadChats = async () => {
  loadingChats.value = true

  //Load chats
  const chats = await messageService.getAllChats()
  chatMenuItems.value = chats.map<IChatMenuItem>((c) => ({
    key: '' + c.id,
    command: () => selectChat(c),
    chat: c
  }))

  let existingChat: ChatListItemDto | undefined
  if (currentChat.value) {
    existingChat = chats.find((c) => c.id === currentChat.value!.id)
  } else if (chatId != null) {
    //If link to existing chat try find chat
    existingChat = chats.find((c) => c.id === chatId)
  } else if (newChatToAdvertisementId) {
    //If trying to create new chat, check if chat does not already exists for given advertisement
    existingChat = chats.find((c) => c.advertisementId === newChatToAdvertisementId)
  }

  if (existingChat) {
    selectChat(existingChat)
  }
  loadingChats.value = false
}

/** Open file select dialog, to select message attachments */
const selectAttachments = () => {
  fileInputEl.value?.openDialog()
}

/**
 * Call Api to send new message
 */
const sendMessage = handleSubmit(async () => {
  sendingMessage.value = true
  try {
    const attachmentFileParameter: FileParameter[] =
      values.attachments?.map((a) => ({
        data: a,
        fileName: a.name
      })) ?? []

    if (!currentChat.value) {
      await messageService.createChat(
        newChatToUserId,
        newChatToAdvertisementId,
        new SendMessageRequest({
          text: values.text,
          attachments: attachmentFileParameter
        } as ISendMessageRequest)
      )
    } else {
      await messageService.sendMessage(currentChat.value?.id, values.text, attachmentFileParameter)
    }
    resetForm()
  } catch (e) {
    handleErrors(e)
  }
  chatMessageEl.value?.scrollToBottom()
  sendingMessage.value = false
})

/**
 * Push new chat item to chat list
 * @param iNewChat
 */
const handleNewChat = (iNewChat: IChatListItemDto) => {
  const newChat = new ChatListItemDto()
  newChat.init(iNewChat)

  const newMenuItem = {
    key: '' + newChat.id,
    command: () => selectChat(newChat),
    chat: newChat
  } as IChatMenuItem

  if (newChat.advertisementOwnerId !== currentUserId.value) {
    newChat.unreadMessageCount = 0
  }

  //Add chat to chat menu item list
  if (chatMenuItems.value.length === 1 && chatMenuItems.value[0].chat == null) {
    chatMenuItems.value = [newMenuItem]
  } else {
    chatMenuItems.value.unshift(newMenuItem)
  }

  //If created new chat load its messages
  if (newChat.advertisementId === newChatToAdvertisementId) {
    selectChat(newChat)
  }
}

/**
 * Update unread message count if needed, and set last message
 * @param chatId
 * @param iNewMessage
 */
const handleNewMessage = (chatId: number, iNewMessage: IMessageItemDto) => {
  if (iNewMessage.fromUserId === currentUserId.value) {
    return
  }

  let chat = chatMenuItems.value.find((i) => i.chat?.id === chatId)?.chat
  if (chat) {
    if (chat.id === currentChat.value?.id) {
      setTimeout(() => {
        chat.unreadMessageCount = chat.unreadMessageCount ? chat.unreadMessageCount + 1 : 1
        chat.lastMessage = iNewMessage.text
      }, NewMessageTimeout)
    } else {
      chat.unreadMessageCount = chat.unreadMessageCount ? chat.unreadMessageCount + 1 : 1
      chat.lastMessage = iNewMessage.text
    }
  }
}

/**
 * Update unread message count
 * @param chatId
 * @param messageIds
 */
const handleMessageRead = (
  chatId: number,
  userId: number,
  _messageIds: number[],
  messagesAffected: number
) => {
  if (userId !== currentUserId.value || !messagesAffected) {
    return
  }

  let chat = chatMenuItems.value.find((i) => i.chat.id === chatId)?.chat
  if (chat) {
    chat.unreadMessageCount = (chat.unreadMessageCount ?? 0) - messagesAffected
  }
}
</script>
