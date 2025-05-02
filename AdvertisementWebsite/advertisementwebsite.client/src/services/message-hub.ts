import * as signalR from '@microsoft/signalr'
import { ref, watch } from 'vue'
import type { IChatListItemDto, IMessageItemDto } from './api-client'
import { AuthService } from './auth-service'

type MessageHubEventName = 'SendNewMessage' | 'SendNewChat' | 'MarkMessageAsRead'
type SubscriptionMap = {
  [eventName in MessageHubEventName]: {
    [callbackKey: string]: (...args: any[]) => void
  }
}

export class MessageHub {
  private authService = AuthService.get()

  private connection: signalR.HubConnection | null = null

  private startConnectPromise: Promise<void> | null = null
  private stopConnectionPromise: Promise<void> | null = null

  private subscriptionIdCounter = 0
  private subscriptions: SubscriptionMap = {
    SendNewMessage: {},
    SendNewChat: {},
    MarkMessageAsRead: {}
  }
  private reconnectSubscriptions: { [key: string]: () => void } = {}

  public connectionStatus = ref<'Connected' | 'Connecting' | 'Disconnected'>('Disconnected')

  constructor() {
    watch(
      AuthService.isAuthenticated,
      (isAuthenticated) => {
        if (isAuthenticated) {
          this.tryConnect()
        } else {
          this.stop()
        }
      },
      { immediate: true }
    )
  }

  /**
   * Build signalR connection
   */
  private buildConnection() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/api/hubs/messages', {
        accessTokenFactory: () => this.authService.tokenInfo?.accessToken!
      })
      .withAutomaticReconnect()
      .build()

    this.connection.onreconnected(() => {
      this.connectionStatus.value = 'Connected'
      for (const callback of Object.values(this.reconnectSubscriptions)) {
        callback()
      }
    })
    this.connection.onreconnecting(() => {
      this.connectionStatus.value = 'Connecting'
    })
    this.connection.onclose(() => {
      this.connectionStatus.value = 'Disconnected'
    })

    this.setEventHandler('SendNewMessage')
    this.setEventHandler('SendNewChat')
    this.setEventHandler('MarkMessageAsRead')
  }

  /**
   * Ensure there is a connection to hub, if user is authorized
   * @returns Promise to boolean which indicates, if could connect to the hub
   */
  public async tryConnect() {
    if (!AuthService.isAuthenticated.value) {
      this.connectionStatus.value = 'Disconnected'
      return false
    }

    if (!this.connection) {
      this.buildConnection()
    }

    if (this.stopConnectionPromise) {
      await this.stopConnectionPromise
    }

    if (this.connection?.state === 'Disconnected') {
      try {
        this.connectionStatus.value = 'Connecting'
        this.startConnectPromise = this.connection.start()
        await this.startConnectPromise
        this.startConnectPromise = null
      } catch (e) {
        console.error(e)
      }
    }

    if (this.startConnectPromise) {
      await this.startConnectPromise
    }

    if (this.connection?.state === 'Connected') {
      this.connectionStatus.value = 'Connected'
      return true
    } else {
      //Could not connect
      this.connectionStatus.value = 'Disconnected'
      return false
    }
  }

  /**
   * Stop connection to hub
   */
  public async stop() {
    if (this.connection) {
      this.stopConnectionPromise = this.connection.stop()
    }
    await this.stopConnectionPromise
    this.stopConnectionPromise = null
  }

  /**
   * Set message hub event handler, which on event will call registered callbacks
   * @param name Hub event name
   */
  private setEventHandler(name: MessageHubEventName) {
    this.connection!.on(name, (...args: unknown[]) => {
      const eventCallbacks = Object.values(this.subscriptions[name])
      for (const callback of eventCallbacks) {
        callback(...args)
      }
    })
  }

  /**
   * Subscribe to message hub event
   * @param eventName Message hub event name
   * @param callback Callback which will be called on event
   * @returns Unsubscribe callback
   */
  private async subscribeToHubEvent(
    eventName: MessageHubEventName,
    callback: (...args: any[]) => void
  ): Promise<() => void> {
    await this.tryConnect()
    const callbackId = '' + this.subscriptionIdCounter++
    this.subscriptions[eventName][callbackId] = callback

    return () => {
      delete this.subscriptions[eventName][callbackId]
    }
  }

  /**
   * Subscribe to 'New message' event
   * @param callback Event callback
   * @returns Unsubscribe callback
   */
  public async subscribeNewMessages(
    callback: (chatId: number, newMessage: IMessageItemDto) => void
  ) {
    return await this.subscribeToHubEvent('SendNewMessage', callback)
  }

  /**
   * Subscribe to 'New chat' event
   * @param callback Event callback
   * @returns Unsubscribe callback
   */
  public async subscribeNewChat(callback: (newChat: IChatListItemDto) => void) {
    return await this.subscribeToHubEvent('SendNewChat', callback)
  }

  /**
   * Subscribe to 'Message has been read' event
   * @param callback Event callback
   * @returns Unsubscribe callback
   */
  public async subscribeMarkMessageAsRead(
    callback: (
      chatId: number,
      userId: number,
      messageIds: number[],
      messagesAffected: number
    ) => void
  ) {
    return await this.subscribeToHubEvent('MarkMessageAsRead', callback)
  }

  /**
   * Subscribe to connection reconnect event
   * @param callback Event callback
   * @returns Unsubscribe callback
   */
  public subscribeToReconnect(callback: () => void) {
    const subscriptionId = '' + this.subscriptionIdCounter++
    this.reconnectSubscriptions[subscriptionId] = callback
    return () => {
      delete this.reconnectSubscriptions[subscriptionId]
    }
  }
}
