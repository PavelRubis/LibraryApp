<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

import type { BookDto } from '@/api/generated'
import BookEditorDialog from '@/components/BookEditorDialog.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { getApiErrorMessage } from '@/services/apiError'
import { booksService } from '@/services/books'
import { useUiStore } from '@/stores/ui'
import { formatAuthors, formatDate } from '@/utils/format'
import { tocXmlToHtml } from '@/utils/xml'

const props = defineProps<{ id: string }>()
const router = useRouter()
const ui = useUiStore()
const book = ref<BookDto | null>(null)
const loading = ref(false)
const editorOpen = ref(false)
const deleteDialogOpen = ref(false)
const deleting = ref(false)
const tocHtml = computed(() => tocXmlToHtml(book.value?.tableOfContentsXml))

async function loadBook(): Promise<void> {
  loading.value = true
  try {
    book.value = await booksService.getById(props.id)
  } catch (error) {
    ui.error(getApiErrorMessage(error))
    void router.replace('/books')
  } finally {
    loading.value = false
  }
}

function afterSave(savedBook: BookDto): void {
  book.value = savedBook
}

async function removeBook(): Promise<void> {
  if (!book.value?.id) return
  deleting.value = true
  try {
    await booksService.remove(book.value.id, book.value.rowVersion)
    ui.success('Книга удалена из библиотеки.')
    await router.push('/books')
  } catch (error) {
    ui.error(getApiErrorMessage(error))
  } finally {
    deleting.value = false
  }
}

onMounted(() => void loadBook())
</script>

<template>
  <v-container class="page-container details-page">
    <v-btn class="mb-5" prepend-icon="mdi-arrow-left" to="/books" variant="text">К списку книг</v-btn>
    <v-progress-linear v-if="loading" color="primary" indeterminate rounded />

    <template v-if="book">
      <section class="details-hero">
        <div class="cover" aria-hidden="true">
          <div class="cover__ornament"><v-icon icon="mdi-book-open-variant" /></div>
          <span>{{ book.publicationYear }}</span>
        </div>
        <div class="details-copy">
          <span class="eyebrow">Карточка книги</span>
          <h1>{{ book.title }}</h1>
          <p class="authors">{{ formatAuthors(book.authors) }}</p>
          <div class="metadata">
            <span><v-icon icon="mdi-calendar-blank-outline" /> Издана в {{ book.publicationYear }} году</span>
            <span><v-icon icon="mdi-clock-outline" /> Добавлена {{ formatDate(book.createdAt) }}</span>
          </div>
          <div class="details-actions">
            <v-btn color="primary" prepend-icon="mdi-pencil-outline" variant="flat" @click="editorOpen = true">
              Редактировать
            </v-btn>
            <v-btn color="error" prepend-icon="mdi-delete-outline" variant="text" @click="deleteDialogOpen = true">
              Удалить
            </v-btn>
          </div>
        </div>
      </section>

      <v-card class="toc-card" elevation="0">
        <div class="toc-card__heading">
          <div>
            <span class="eyebrow">Содержание</span>
            <h2>Оглавление</h2>
          </div>
          <v-icon color="primary" icon="mdi-format-list-numbered" size="32" />
        </div>
        <!-- HTML проходит белый список тегов и атрибутов в tocXmlToHtml. -->
        <div v-if="tocHtml" class="toc-content" v-html="tocHtml" />
        <p v-else class="text-medium-emphasis">Оглавление не заполнено.</p>
      </v-card>
    </template>

    <BookEditorDialog v-model="editorOpen" :book="book" @saved="afterSave" />
    <ConfirmDialog
      v-model="deleteDialogOpen"
      :loading="deleting"
      :text="`Книга «${book?.title ?? ''}» будет удалена из библиотеки.`"
      title="Удалить книгу?"
      @confirm="removeBook"
    />
  </v-container>
</template>

<style scoped>
.details-page {
  max-width: 1080px;
}

.details-hero {
  display: grid;
  grid-template-columns: 230px 1fr;
  gap: clamp(32px, 6vw, 72px);
  align-items: center;
  padding: 24px 0 48px;
}

.cover {
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  aspect-ratio: 0.72;
  padding: 25px;
  border-radius: 7px 19px 19px 7px;
  color: #f7ead2;
  background: linear-gradient(135deg, #2f6b60, #173c36);
  box-shadow: -8px 10px 0 #d5c29c, 0 23px 45px rgba(27, 58, 51, 0.2);
}

.cover::before {
  position: absolute;
  inset: 0 auto 0 17px;
  width: 1px;
  background: rgba(255, 255, 255, 0.22);
  content: '';
}

.cover__ornament {
  display: grid;
  place-items: center;
  aspect-ratio: 1;
  border: 1px solid rgba(247, 234, 210, 0.6);
  border-radius: 50%;
  font-size: 48px;
}

.cover > span {
  text-align: center;
  font-family: Georgia, 'Times New Roman', serif;
  letter-spacing: 0.14em;
}

.details-copy h1 {
  max-width: 700px;
  color: #243b35;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(38px, 6vw, 68px);
  font-weight: 500;
  line-height: 1.02;
}

.authors {
  margin: 15px 0 24px;
  color: #a25430;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 21px;
}

.metadata {
  display: flex;
  flex-wrap: wrap;
  gap: 15px 24px;
  color: #65716b;
  font-size: 14px;
}

.metadata span {
  display: inline-flex;
  align-items: center;
  gap: 7px;
}

.details-actions {
  display: flex;
  gap: 10px;
  margin-top: 32px;
}

.toc-card {
  padding: clamp(24px, 5vw, 48px);
  border: 1px solid rgba(36, 91, 82, 0.12);
}

.toc-card__heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 20px;
  margin-bottom: 24px;
  border-bottom: 1px solid #e4e5e0;
}

.toc-card h2 {
  color: #263b35;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 31px;
}

.toc-content {
  color: #3e4d48;
  line-height: 1.8;
}

.toc-content :deep(ul),
.toc-content :deep(ol) {
  padding-left: 25px;
}

.toc-content :deep(a) {
  color: #245b52;
}

@media (max-width: 700px) {
  .details-hero {
    grid-template-columns: 1fr;
  }

  .cover {
    width: min(190px, 60vw);
    margin: 0 auto;
  }
}
</style>
