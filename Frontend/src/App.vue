<script setup lang="ts">
import AppHeader from '@/components/AppHeader.vue'
import { useUiStore } from '@/stores/ui'

const ui = useUiStore()
</script>

<template>
  <v-app>
    <AppHeader />

    <v-main>
      <router-view />
    </v-main>

    <v-footer class="app-footer">
      <span>Домашняя библиотека</span>
      <v-spacer />
      <span>{{ new Date().getFullYear() }}</span>
    </v-footer>

    <v-snackbar v-model="ui.snackbar" :color="ui.messageType" location="bottom right" timeout="5000">
      {{ ui.message }}
      <template #actions>
        <v-btn variant="text" @click="ui.snackbar = false">Закрыть</v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>
