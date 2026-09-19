<script setup lang="ts">
import { nextTick, onMounted, ref, watch } from 'vue'

import { htmlToTocXml, tocXmlToHtml, validateTocXml } from '@/utils/xml'

const props = defineProps<{
  modelValue: string
  label?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const editor = ref<HTMLElement | null>(null)
const error = ref<string>('')
let lastEmittedValue = ''

function syncFromModel(): void {
  if (!editor.value || props.modelValue === lastEmittedValue) return
  editor.value.innerHTML = tocXmlToHtml(props.modelValue)
}

function emitXml(): void {
  if (!editor.value) return
  const xml = htmlToTocXml(editor.value.innerHTML)
  lastEmittedValue = xml
  const validation = validateTocXml(xml)
  error.value = validation === true ? '' : validation
  emit('update:modelValue', xml)
}

function applyCommand(command: string, value?: string): void {
  editor.value?.focus()
  document.execCommand(command, false, value)
  emitXml()
}

function applyBlock(event: Event): void {
  const value = (event.target as HTMLSelectElement).value
  if (value) applyCommand('formatBlock', value)
}

function addLink(): void {
  const href = window.prompt('Адрес ссылки')
  if (href && !/^\s*(javascript|data|vbscript):/i.test(href)) {
    applyCommand('createLink', href)
  }
}

onMounted(() => nextTick(syncFromModel))
watch(() => props.modelValue, () => nextTick(syncFromModel))
</script>

<template>
  <div class="rich-editor" :class="{ 'rich-editor--error': error }">
    <div class="rich-editor__label">{{ label ?? 'Оглавление' }}</div>
    <div class="rich-editor__toolbar" role="toolbar" aria-label="Форматирование оглавления">
      <select aria-label="Стиль абзаца" @change="applyBlock">
        <option value="p">Обычный текст</option>
        <option value="h2">Заголовок</option>
        <option value="h3">Подзаголовок</option>
      </select>
      <button type="button" title="Полужирный" @click="applyCommand('bold')"><strong>B</strong></button>
      <button type="button" title="Курсив" @click="applyCommand('italic')"><em>I</em></button>
      <button type="button" title="Подчёркнутый" @click="applyCommand('underline')"><u>U</u></button>
      <button type="button" title="Маркированный список" @click="applyCommand('insertUnorderedList')">
        <v-icon icon="mdi-format-list-bulleted" size="small" />
      </button>
      <button type="button" title="Нумерованный список" @click="applyCommand('insertOrderedList')">
        <v-icon icon="mdi-format-list-numbered" size="small" />
      </button>
      <button type="button" title="Добавить ссылку" @click="addLink">
        <v-icon icon="mdi-link-variant" size="small" />
      </button>
      <button type="button" title="Очистить форматирование" @click="applyCommand('removeFormat')">
        <v-icon icon="mdi-format-clear" size="small" />
      </button>
    </div>
    <div
      ref="editor"
      class="rich-editor__content"
      contenteditable="true"
      data-placeholder="Например: Глава 1. Начало..."
      role="textbox"
      aria-multiline="true"
      @blur="emitXml"
      @input="emitXml"
    />
    <div v-if="error" class="rich-editor__error">{{ error }}</div>
    <div v-else class="rich-editor__hint">Содержимое сохраняется как безопасный XML с корнем &lt;toc&gt;.</div>
  </div>
</template>

<style scoped>
.rich-editor {
  border: 1px solid #777f7a;
  border-radius: 12px;
  overflow: hidden;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.rich-editor:focus-within {
  border-color: rgb(var(--v-theme-primary));
  box-shadow: 0 0 0 1px rgb(var(--v-theme-primary));
}

.rich-editor--error {
  border-color: rgb(var(--v-theme-error));
}

.rich-editor__label {
  color: #56625d;
  font-size: 12px;
  padding: 8px 13px 0;
}

.rich-editor__toolbar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  padding: 8px;
  border-bottom: 1px solid #dde1dd;
  background: #f7f5ef;
}

.rich-editor__toolbar button,
.rich-editor__toolbar select {
  min-width: 34px;
  height: 32px;
  padding: 0 9px;
  border: 0;
  border-radius: 7px;
  color: #334640;
  background: transparent;
  cursor: pointer;
}

.rich-editor__toolbar button:hover,
.rich-editor__toolbar select:hover {
  background: #e5eee9;
}

.rich-editor__content {
  min-height: 190px;
  max-height: 340px;
  overflow-y: auto;
  padding: 14px 16px;
  outline: none;
  line-height: 1.65;
  background: #fff;
}

.rich-editor__content:empty::before {
  color: #9ca39f;
  content: attr(data-placeholder);
}

.rich-editor__hint,
.rich-editor__error {
  padding: 5px 13px 8px;
  font-size: 12px;
}

.rich-editor__hint {
  color: #707b76;
}

.rich-editor__error {
  color: rgb(var(--v-theme-error));
}
</style>
