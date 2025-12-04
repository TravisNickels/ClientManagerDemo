<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Client } from '@/types/client'

const props = defineProps<{
  clients: Client[]
}>()

const filterOptions = [
  { value: 'all', label: 'All' },
  { value: 'active', label: 'Active' },
  { value: 'archived', label: 'Archived' },
] as const

const sortOptions = [
  { value: 'az', label: 'A → Z' },
  { value: 'za', label: 'Z → A' },
  { value: 'active', label: 'Active' },
  { value: 'archived', label: 'Archived' },
] as const

const emit = defineEmits<{
  (e: 'create'): void
  (e: 'update', clients: Client[], showingArchive: boolean): void
}>()

const storedFilterOption = localStorage.getItem('filterOption')
const storedSortOption = localStorage.getItem('sortOption')
const storedSearchQuery = localStorage.getItem('searchQuery')

type FilterValue = (typeof filterOptions)[number]['value']
type SortValue = (typeof sortOptions)[number]['value']

const searchQuery = ref<string>(storedSearchQuery ?? '')
const filterOption = ref<FilterValue>((storedFilterOption as FilterValue) ?? 'all')
const sortOption = ref<SortValue>((storedSortOption as SortValue) ?? 'az')

const selectedFilterLabel = computed(() => filterOptions.find((o) => o.value === filterOption.value)?.label ?? 'All')
const selectedSortLabel = computed(() => sortOptions.find((o) => o.value === sortOption.value)?.label ?? 'A → Z')

const matchesSearch = (client: Client) => {
  const full = `${client.firstName} ${client.lastName}`.toLowerCase()
  return full.includes(searchQuery.value.toLowerCase())
}

const filtered = computed(() => {
  let result = props.clients

  if (filterOption.value === 'active') result = result.filter((c) => !c.isArchived)
  if (filterOption.value === 'archived') result = result.filter((c) => c.isArchived)

  return result.filter(matchesSearch)
})

const sorted = computed(() => {
  return [...filtered.value].sort((a, b) => {
    switch (sortOption.value) {
      case 'az':
        return a.lastName.localeCompare(b.lastName)
      case 'za':
        return b.lastName.localeCompare(a.lastName)
      case 'active':
        return Number(a.isArchived) - Number(b.isArchived)
      case 'archived':
        return Number(b.isArchived) - Number(a.isArchived)
      default:
        return 0
    }
  })
})

watch(sortOption, () => {
  localStorage.setItem('sortOption', sortOption.value)
})

watch(sorted, (value) => {
  localStorage.setItem('filterOption', filterOption.value)
  localStorage.setItem('sortOption', sortOption.value)
  localStorage.setItem('searchQuery', searchQuery.value)
  const showingArchive = filterOption.value === 'archived'
  emit('update', value, showingArchive)
})
</script>

<template>
  <div class="d-flex align-items-center gap-2 p-3 border-bottom bg-white">
    <input v-model="searchQuery" type="text" class="form-control" placeholder="Search clients..." />

    <!-- Filter -->
    <div class="dropdown">
      <button class="btn dropdown-toggle form-control" data-bs-toggle="dropdown">
        {{ selectedFilterLabel }}
      </button>

      <ul class="dropdown-menu">
        <li v-for="option in filterOptions" :key="option.value">
          <button
            type="button"
            class="dropdown-item d-flex justify-content-between"
            :class="{ active: option.value === filterOption }"
            :aria-label="'option-' + option.value"
            @click="filterOption = option.value"
          >
            {{ option.label }}
            <i v-if="option.value === filterOption" class="bi bi-check"></i>
          </button>
        </li>
      </ul>
    </div>

    <!-- Sorting -->
    <div class="dropdown">
      <button class="btn dropdown-toggle w-auto" data-bs-toggle="dropdown">
        {{ selectedSortLabel }}
      </button>

      <ul class="dropdown-menu">
        <li v-for="option in sortOptions" :key="option.value">
          <button
            type="button"
            class="dropdown-item d-flex justify-content-between"
            :class="{ active: option.value === sortOption }"
            @click="sortOption = option.value"
          >
            {{ option.label }}
            <i v-if="option.value === sortOption" class="bi bi-check"></i>
          </button>
        </li>
      </ul>
    </div>

    <button class="btn btn-primary ms-auto text-nowrap" @click="emit('create')">+ Create</button>
  </div>
</template>
