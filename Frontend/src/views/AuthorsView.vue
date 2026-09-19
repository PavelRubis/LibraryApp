<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'

import type { AuthorDto } from '@/api/generated'
import AuthorEditorDialog from '@/components/AuthorEditorDialog.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { getApiErrorMessage } from '@/services/apiError'
import { authorsService } from '@/services/authors'
import { useUiStore } from '@/stores/ui'
import { formatDate } from '@/utils/format'

const ui = useUiStore()
const authors = ref<AuthorDto[]>([])
const query = ref('')
const page = ref(1)
const pageCount = ref(0)
const totalCount = ref(0)
const loading = ref(false)
const editorOpen = ref(false)
const deleteDialogOpen = ref(false)
const deleting = ref(false)
const selectedAuthor = ref<AuthorDto | null>(null)

async function loadAuthors(): Promise<void> {
  loading.value = true
  try {
    const result = await authorsService.search(query.value.trim(), page.value, 15)
    authors.value = result.items ?? []
    pageCount.value = result.pageCount ?? 0
    totalCount.value = result.totalCount ?? 0
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    loading.value = false
  }
}

function search(): void {
  if (page.value !== 1) page.value = 1
  else void loadAuthors()
}

function openCreate(): void {
  selectedAuthor.value = null
  editorOpen.value = true
}

function openEdit(author: AuthorDto): void {
  selectedAuthor.value = author
  editorOpen.value = true
}

function openDelete(author: AuthorDto): void {
  selectedAuthor.value = author
  deleteDialogOpen.value = true
}

async function removeAuthor(): Promise<void> {
  if (!selectedAuthor.value?.id) return
  deleting.value = true
  try {
    await authorsService.remove(selectedAuthor.value.id, selectedAuthor.value.rowVersion)
    ui.success('Автор удалён.')
    deleteDialogOpen.value = false
    selectedAuthor.value = null
    if (authors.value.length === 1 && page.value > 1) page.value -= 1
    else await loadAuthors()
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    deleting.value = false
  }
}

async function afterSave(): Promise<void> {
  await loadAuthors()
}

watch(page, () => void loadAuthors())
onMounted(() => void loadAuthors())
</script>

<template>
  <v-container class="page-container authors-page">
    <section class="hero-section">
      <div>
        <span class="eyebrow">Справочник</span>
        <h1>Авторы</h1>
        <p>Управляйте авторами, которых можно назначить книгам.</p>
      </div>
      <v-btn color="primary" prepend-icon="mdi-account-plus" size="large" variant="flat" @click="openCreate">
        Добавить автора
      </v-btn>
    </section>

    <v-card class="authors-card" elevation="0">
      <div class="authors-toolbar">
        <v-form class="author-search" @submit.prevent="search">
          <v-text-field
            v-model="query"
            clearable
            hide-details
            label="Поиск автора"
            prepend-inner-icon="mdi-magnify"
            @click:clear="search"
          />
          <v-btn color="primary" height="48" type="submit" variant="tonal">Найти</v-btn>
        </v-form>
        <span>{{ totalCount }} записей</span>
      </div>

      <v-progress-linear v-if="loading" color="primary" indeterminate />

      <v-table v-if="authors.length" class="authors-table" hover>
        <thead>
          <tr>
            <th>Автор</th>
            <th>Добавлен</th>
            <th class="text-right">Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="author in authors" :key="author.id">
            <td>
              <div class="author-name">
                <span>{{ author.name?.charAt(0).toUpperCase() }}</span>
                <strong>{{ author.name }}</strong>
              </div>
            </td>
            <td class="text-medium-emphasis">{{ formatDate(author.createdAt) }}</td>
            <td class="text-right">
              <v-btn aria-label="Редактировать" icon="mdi-pencil-outline" size="small" variant="text" @click="openEdit(author)" />
              <v-btn
                aria-label="Удалить"
                color="error"
                icon="mdi-delete-outline"
                size="small"
                variant="text"
                @click="openDelete(author)"
              />
            </td>
          </tr>
        </tbody>
      </v-table>

      <div v-else-if="!loading" class="empty-state compact">
        <v-icon color="primary" icon="mdi-account-search-outline" size="54" />
        <h2>{{ query ? 'Авторы не найдены' : 'Список авторов пуст' }}</h2>
        <p>{{ query ? 'Попробуйте другой запрос.' : 'Добавьте автора, чтобы создать книгу.' }}</p>
      </div>

      <v-pagination
        v-if="pageCount > 1"
        v-model="page"
        class="py-5"
        color="primary"
        :length="pageCount"
        :total-visible="7"
      />
    </v-card>

    <AuthorEditorDialog v-model="editorOpen" :author="selectedAuthor" @saved="afterSave" />
    <ConfirmDialog
      v-model="deleteDialogOpen"
      :loading="deleting"
      :text="`Автор «${selectedAuthor?.name ?? ''}» будет удалён. Если он связан с книгами, сервер отклонит операцию.`"
      title="Удалить автора?"
      @confirm="removeAuthor"
    />
  </v-container>
</template>

<style scoped>
.authors-page {
  max-width: 1080px;
}

.authors-card {
  overflow: hidden;
  border: 1px solid rgba(36, 91, 82, 0.12);
}

.authors-toolbar {
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 18px 20px;
  border-bottom: 1px solid #e4e5e0;
}

.authors-toolbar > span {
  color: #758079;
  white-space: nowrap;
}

.author-search {
  display: grid;
  grid-template-columns: minmax(220px, 560px) auto;
  gap: 10px;
  flex: 1;
}

.authors-table th {
  color: #66726c !important;
  font-size: 12px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.author-name {
  display: flex;
  align-items: center;
  gap: 13px;
}

.author-name > span {
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border-radius: 50%;
  color: #245b52;
  background: #e5eee9;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 18px;
}

@media (max-width: 600px) {
  .authors-toolbar,
  .hero-section {
    align-items: stretch;
    flex-direction: column;
  }

  .author-search {
    grid-template-columns: 1fr;
  }
}
</style>
