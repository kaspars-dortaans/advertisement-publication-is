<template>
  <div class="relative flex-1 h-0 flex flex-col flex-nowrap">
    <div class="flex gap-2 py-2">
      <slot name="buttons"></slot>

      <Button
        v-if="currentAdvertisementId"
        :label="l.messages.viewAdvertisement"
        as="RouterLink"
        :to="{ name: 'viewAdvertisement', params: { id: currentAdvertisementId } }"
      />
    </div>

    <!-- Connection error messages -->
    <div
      v-if="messageHub.connectionStatus.value === 'Disconnected'"
      class="flex items-baseline gap-2 justify-center"
    >
      <p class="rounded-md p-1.5 w-fit bg-red-400 mb-2">
        {{ l.messages.noConnectionToServer }}
      </p>
      <Button
        :label="l.messages.retryConnecting"
        severity="secondary"
        size="small"
        @click="() => messageHub.tryConnect()"
      />
    </div>
    <p
      v-else-if="messageHub.connectionStatus.value === 'Connecting'"
      class="rounded-md p-1.5 w-fit mx-auto bg-amber-200 mb-2"
    >
      {{ l.messages.connectingToServer }}
    </p>
    <!-- No chat selected / no messages yet -->
    <p
      v-else-if="!loading && !messagesByDate.length"
      class="rounded-md p-1.5 w-fit mx-auto bg-amber-200 mb-2"
    >
      {{ !currentChatId && !isNewChat ? l.messages.noChatSelected : l.messages.noMessageYet }}
    </p>

    <!-- Message container -->
    <div
      class="flex-1 overflow-y-auto flex flex-col flex-nowrap gap-2 pr-2"
      ref="messageContainer"
      @scroll="handleScroll.debounce"
    >
      <p
        v-if="typeof stickyDateGroupIndex === 'number'"
        class="self-center rounded-2xl py-1 px-3 bg-surface-200 absolute top"
      >
        {{ dateFormat.format(messagesByDate[stickyDateGroupIndex]?.date) }}
      </p>

      <!-- Message groups by date -->
      <template v-for="messageDateGroup in messagesByDate" :key="messageDateGroup.date">
        <p class="self-center rounded-2xl py-1 px-3 bg-surface-200" ref="messageDates">
          {{ dateFormat.format(messageDateGroup.date) }}
        </p>

        <!-- Messages -->
        <div
          v-for="message in messageDateGroup.messages"
          :key="message.id"
          class="message"
          :class="message.fromUserId == currentUserId ? 'outgoing' : 'incoming'"
          :id="
            message.isMessageRead ? undefined : messageIdPrefix + messageIdSeparator + message.id
          "
          ref="message"
        >
          <!-- Message attachments -->
          <div
            v-for="attachment in message.attachments"
            :key="attachment.url"
            class="message-attachment"
          >
            <Button
              v-if="typeof attachmentInfo[attachment.url!].downloadPercentage === 'number'"
              :loading="true"
              :label="attachmentInfo[attachment.url!].downloadPercentage + '%'"
              class="attachment-download-button"
            />
            <Button
              v-else
              icon="pi pi-download "
              class="attachment-download-button"
              @click="downloadAttachment(attachment)"
            />

            <div class="attachment-text-container">
              <p class="clamp-line">{{ attachment.fileName }}</p>
              <p>{{ attachmentInfo[attachment.url!].size }}</p>
            </div>
          </div>

          <!-- Message text -->
          <p>
            <span class="break-all">{{ message.text }}</span>
            <span class="float-end ml-3">
              <span class="text-xs inline-flex gap-1.5">
                <span class="whitespace-nowrap">{{ timeFormat.format(message.sentTime) }}</span>
                <span v-if="message.fromUserId == currentUserId">
                  {{ message.isMessageRead ? l.messages.messageIsRead : l.messages.messageIsSent }}
                </span>
              </span>
            </span>
          </p>
        </div>
      </template>

      <Button
        v-if="isScrollingPreviousMessages"
        :badge="unreadMessageCount ? '' + unreadMessageCount : ''"
        badgeSeverity="primary"
        icon="pi pi-arrow-down"
        severity="secondary"
        class="absolute bottom-3 left-1/2 -translate-x-1/2 min-w-10 min-h-10"
        rounded
        :pt="{ label: 'hidden', pcBadge: { root: '!min-w-6 !min-h-6 rounded-2xl' } }"
        @click="scrollMessageContainerToBottom"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { pxToMarkAsScrollingPreviousMessages, scrollHandleDebounce } from '@/constants/message'
import {
  MessageAttachmentItemDto,
  MessageClient,
  MessageItemDto,
  type IMessageItemDto
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { MessageHub } from '@/services/message-hub'
import type { IMessageDateGroup } from '@/types/messages/message-date-group'
import { getClient } from '@/utils/client-builder'
import { debounceFn } from '@/utils/debounce'
import { downloadAndSaveFile, formatDataSize } from '@/utils/file-helper'
import {
  computed,
  inject,
  nextTick,
  onBeforeMount,
  onBeforeUnmount,
  ref,
  useTemplateRef,
  watch
} from 'vue'

//Model
const loading = defineModel<boolean>('loading')

//Props
const props = defineProps<{
  currentChatId?: number
  currentAdvertisementId?: number
  isNewChat: boolean
}>()

//Services
const l = LocaleService.currentLocale
const messageHub = inject<MessageHub>('messageHub')!
const messageService = getClient(MessageClient)

//Refs
const messageContainer = useTemplateRef('messageContainer')
const messageDateElements = useTemplateRef('messageDates')
const messagesElements = useTemplateRef('message')

//Constants
const messageIdPrefix = 'message'
const messageIdSeparator = '-'

//Reactive data
const currentUserId = computed(() => AuthService.profileInfo.value?.id)
const isScrollingPreviousMessages = ref<boolean>(false)
const messagesByDate = ref<IMessageDateGroup[]>([])
const unreadMessageCount = ref(0)
const messageObserver = ref<IntersectionObserver | undefined>()
const stickyDateGroupIndex = ref<number | undefined>()
const attachmentInfo = ref<{
  [key: string]: {
    size: string
    downloadPercentage?: number
  }
}>({})

const unsubscribeNewMessage = ref<(() => void) | undefined>()
const unsubscribeMarkMessageAsRead = ref<(() => void) | undefined>()
const unsubscribeOnReconnect = ref<(() => void) | undefined>()

const timeFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    timeStyle: 'short'
  })
)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'long'
  })
)

//Hooks
onBeforeMount(async () => {
  messageObserver.value = new IntersectionObserver(observerMessage, {
    root: messageContainer.value,
    threshold: 1
  })

  unsubscribeNewMessage.value = await messageHub.subscribeNewMessages(handleNewMessage)
  unsubscribeMarkMessageAsRead.value =
    await messageHub.subscribeMarkMessageAsRead(handleMessageRead)
  unsubscribeOnReconnect.value = messageHub.subscribeToReconnect(() => {
    loadMessages(props.currentChatId)
  })

  if (props.currentChatId) {
    loadMessages(props.currentChatId)
  }
})

onBeforeUnmount(async () => {
  unsubscribeNewMessage.value?.()
  unsubscribeMarkMessageAsRead.value?.()
  unsubscribeOnReconnect.value?.()
})

watch(
  () => props.currentChatId,
  async (chatId) => {
    await loadMessages(chatId)
  }
)

//Methods
/**
 * Call Api to load given chat messages
 * @param chat
 */
const loadMessages = async (chatId?: number) => {
  loading.value = true

  //Remove stop observing old messages
  messageObserver.value?.disconnect()

  //Reset values
  attachmentInfo.value = {}
  messagesByDate.value = []
  unreadMessageCount.value = 0

  //Load messages
  if (chatId) {
    const messageDictionary = await messageService.getChatMessages(chatId)
    const messageDateGroups: IMessageDateGroup[] = []
    const unreadMessageIds: number[] = []
    for (const dateString in messageDictionary) {
      for (const message of messageDictionary[dateString]) {
        if (
          !message.isMessageRead &&
          message.fromUserId !== currentUserId.value &&
          typeof message.id === 'number'
        ) {
          unreadMessageIds.push(message.id)
        }

        if (message.attachments) {
          for (const attachment of message.attachments) {
            attachmentInfo.value[attachment.url!] = {
              size: formatDataSize(attachment.sizeInBytes!)
            }
          }
        }
      }

      messageDateGroups.push({
        date: new Date(dateString),
        messages: messageDictionary[dateString]
      })
    }
    messagesByDate.value = messageDateGroups.sort((a, b) => (a.date < b.date ? -1 : 1))

    //Mark chat messages as read
    if (unreadMessageIds.length) {
      markMessageAsRead(unreadMessageIds)
    }
  }

  //Scroll last message
  nextTick(() => {
    scrollMessageContainerToBottom()
  })

  loading.value = false
}

/**
 * Update unread message count if needed,
 * If message is for current chat, push to current chat messages
 * @param chatId
 * @param iNewMessage
 */
const handleNewMessage = (chatId: number, iNewMessage: IMessageItemDto) => {
  //If message not for current chat return
  if (props.currentChatId !== chatId) {
    return
  }

  const newMessage = new MessageItemDto()
  newMessage.init(iNewMessage)

  //Add attachment info
  if (newMessage.attachments?.length) {
    for (const attachment of newMessage.attachments)
      attachmentInfo.value[attachment.url!] = {
        size: formatDataSize(attachment.sizeInBytes!)
      }
  }

  //Find message group
  const newMessageDate = new Date(newMessage.sentTime!)
  newMessageDate.setMilliseconds(0)
  newMessageDate.setMinutes(0)
  newMessageDate.setSeconds(0)
  newMessageDate.setHours(0)

  const dateMilliseconds = newMessageDate.getTime()
  let dateIndex = messagesByDate.value.findIndex((md) => md.date.getTime() == dateMilliseconds)
  if (dateIndex === -1) {
    //Create new date group
    messagesByDate.value.push({
      date: newMessageDate,
      messages: [newMessage]
    })
  } else {
    //Add to group
    messagesByDate.value[dateIndex].messages.push(newMessage)
  }

  //If not current user message
  if (newMessage.fromUserId !== currentUserId.value) {
    //Scroll to view new message & mark it as read
    if (!isScrollingPreviousMessages.value) {
      markMessageAsRead([newMessage.id!])
      nextTick(() => {
        scrollMessageContainerToBottom()
      })
    } else {
      unreadMessageCount.value++
      //When new message is displayed observe it
      nextTick(() => {
        const messageEl = messagesElements.value?.find((el) => el.id === 'message-' + newMessage.id)
        if (messageEl) {
          messageObserver.value?.observe(messageEl)
        }
      })
    }
  }
}

/**
 * Change message status to read
 * @param chatId
 * @param messageIds
 */
const handleMessageRead = (
  chatId: number,
  _userId: number,
  messageIds: number[],
  messagesAffected: number
) => {
  if (props.currentChatId !== chatId || !messagesAffected) {
    return
  }

  let unreadMessagesCounter = 0
  for (let i = messagesByDate.value.length - 1; i >= 0; i--) {
    for (let j = messagesByDate.value[i].messages.length - 1; j >= 0; j--) {
      if (messagesByDate.value[i].messages[j].isMessageRead) {
        continue
      }

      //If read message ids contain current message id, mark it as read
      if (messageIds.some((id) => id === messagesByDate.value[i].messages[j].id)) {
        messagesByDate.value[i].messages[j].isMessageRead = true
      } else if (messagesByDate.value[i].messages[j].fromUserId !== currentUserId.value) {
        //If not, and current user id is not equal to message sender, increase unread message counter
        unreadMessagesCounter++
      }
    }
  }
  unreadMessageCount.value = unreadMessagesCounter
}

/**
 * Scroll to message container bottom
 */
const scrollMessageContainerToBottom = () => {
  if (messageContainer.value) {
    messageContainer.value.scrollTo(0, messageContainer.value.scrollHeight)
  }
}

/**
 * Set scrolling flag, set topmost visible message group index
 */
const handleScroll = debounceFn(() => {
  const el = messageContainer.value
  if (!el?.scrollHeight) {
    isScrollingPreviousMessages.value = false
    stickyDateGroupIndex.value = undefined
  } else {
    isScrollingPreviousMessages.value =
      el.scrollHeight - el.scrollTop - el.clientHeight > pxToMarkAsScrollingPreviousMessages

    stickyDateGroupIndex.value = searchStickyDateElement(stickyDateGroupIndex.value)
  }
}, scrollHandleDebounce)

/**
 * Search for messageByDate topmost visible group index
 * @param lastIndex
 */
const searchStickyDateElement = (lastIndex: number | undefined) => {
  if (!messageDateElements.value?.length) {
    return undefined
  }

  const elements = messageDateElements.value
  if (typeof lastIndex !== 'number' || lastIndex > elements.length - 1) {
    lastIndex = elements.length - 1
  }
  const scrollTop = messageContainer.value!.scrollTop

  if (elements[lastIndex].offsetTop === scrollTop) {
    return lastIndex
  }

  const delta = elements[lastIndex].offsetTop < scrollTop ? 1 : -1
  for (let i = lastIndex; i > -1 && i < elements.length; i += delta) {
    const elementAboveScrollTop = elements[i].offsetTop < scrollTop

    if (delta > 0 && !elementAboveScrollTop) {
      //If searching for first element below scroll top return previous element index
      return i - 1
    } else if (delta < 0 && elementAboveScrollTop) {
      //If searching for first element above scroll top return this element index
      return i
    }
  }

  return delta > 0 ? elements.length - 1 : 0
}

/**
 * Observe unread messages, mark as read when they become visible for user
 * @param entries
 * @param observer
 */
const observerMessage: IntersectionObserverCallback = (entries, observer) => {
  const readMessageIds = []
  for (const e of entries) {
    if (!e.isIntersecting) {
      continue
    }

    const id = e.target.id.startsWith(messageIdPrefix)
      ? parseInt(e.target.id.split(messageIdSeparator)[1])
      : null
    if (id != null && !isNaN(id)) {
      readMessageIds.push(id)
    } else {
      observer.unobserve(e.target)
    }
  }
  if (readMessageIds.length) {
    markMessageAsRead(readMessageIds)
  }
}

/**
 * Call Api to mark messages as read
 * @param ids message ids
 */
const markMessageAsRead = async (ids: number[]) => {
  if (!props.currentChatId) {
    return
  }

  await messageService.markMessageAsRead(props.currentChatId, ids)
}

/**
 * Download message attachment and display progress
 * @param attachment
 */
const downloadAttachment = async (attachment: MessageAttachmentItemDto) => {
  if (attachmentInfo.value[attachment.url!].downloadPercentage != null) {
    //Download already in progress return
    return
  }

  attachmentInfo.value[attachment.url!].downloadPercentage = 0
  await downloadAndSaveFile(attachment.url!, (e) => {
    attachmentInfo.value[attachment.url!].downloadPercentage = Math.trunc(e.progress! * 100)
  })
  attachmentInfo.value[attachment.url!].downloadPercentage = undefined
}

defineExpose({ scrollToBottom: scrollMessageContainerToBottom })
</script>
