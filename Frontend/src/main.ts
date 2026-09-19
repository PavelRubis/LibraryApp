import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import '@/styles/main.css'

import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import { configureApi } from '@/api/config'
import vuetify from '@/plugins/vuetify'
import router from '@/router'

configureApi()

createApp(App).use(createPinia()).use(router).use(vuetify).mount('#app')
