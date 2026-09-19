import { defineStore } from 'pinia'

type MessageType = 'success' | 'error' | 'info' | 'warning'

export const useUiStore = defineStore('ui', {
  state: () => ({
    snackbar: false,
    message: '',
    messageType: 'info' as MessageType,
  }),
  actions: {
    show(message: string, type: MessageType = 'info') {
      this.message = message
      this.messageType = type
      this.snackbar = true
    },
    success(message: string) {
      this.show(message, 'success')
    },
    error(message: string) {
      this.show(message, 'error')
    },
  },
})
