<script setup lang="ts">
import { useDisplay } from 'vuetify'

const { smAndDown } = useDisplay()
</script>

<template>
  <v-app-bar class="app-header" elevation="0" height="76">
    <v-container class="header-content d-flex align-center">
      <router-link class="brand" to="/books" aria-label="Домашняя библиотека">
        <span class="brand-mark"><v-icon icon="mdi-bookshelf" /></span>
        <span v-if="!smAndDown" class="brand-copy">
          <strong>Домашняя библиотека</strong>
        </span>
      </router-link>

      <v-spacer />

      <nav class="navigation" aria-label="Основная навигация">
        <v-btn to="/books" variant="text" prepend-icon="mdi-book-open-page-variant">
          {{ smAndDown ? '' : 'Книги' }}
        </v-btn>
        <v-btn to="/authors" variant="text" prepend-icon="mdi-account-edit">
          {{ smAndDown ? '' : 'Авторы' }}
        </v-btn>
      </nav>
    </v-container>
  </v-app-bar>
</template>

<style scoped>
.app-header {
  border-bottom: 1px solid rgba(36, 91, 82, 0.12);
  background: rgba(255, 253, 248, 0.94) !important;
  backdrop-filter: blur(14px);
}

.header-content {
  max-width: 1240px;
}

.brand {
  color: #173c36;
  display: inline-flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border-radius: 14px;
  color: #fffdf8;
  background: #245b52;
  font-size: 24px;
}

.brand-copy {
  display: flex;
  flex-direction: column;
  line-height: 1.15;
}

.brand-copy strong {
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 18px;
}

.brand-copy small {
  color: #6c7771;
  margin-top: 4px;
  font-size: 11px;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.navigation {
  display: flex;
  gap: 6px;
}

.navigation :deep(.v-btn--active) {
  color: #245b52;
  background: #e5eee9;
}
</style>
