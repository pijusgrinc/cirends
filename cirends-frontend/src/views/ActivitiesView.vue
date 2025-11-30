<template>
  <div class="activities-page">
    <div class="container">
      <div class="page-header">
        <h1>Mano veiklos</h1>
        <Button variant="primary" @click="openCreateModal">
          <template #icon>
            <Icon name="add" />
          </template>
          Nauja veikla
        </Button>
      </div>

      <Loading v-if="activitiesStore.loading" />

      <div v-else-if="activities.length > 0" class="activities-grid">
        <ActivityCard
          v-for="activity in activities"
          :key="activity.id"
          :activity="activity"
          :showActions="true"
          @click="$router.push(`/activities/${activity.id}`)"
          @edit="editActivity(activity)"
          @delete="handleDelete(activity.id)"
        />
      </div>

      <EmptyState
        v-else
        title="Nėra veiklų"
        description="Sukurkite savo pirmąją veiklą, kad pradėtumėte"
      >
        <Button variant="primary" @click="openCreateModal">
          Sukurti veiklą
        </Button>
      </EmptyState>
    </div>

    <!-- Create/Edit Modal -->
    <Modal v-if="showModal" @close="closeModal">
      <template #header>
        {{ editingActivity ? 'Redaguoti veiklą' : 'Sukurti naują veiklą' }}
      </template>
      <template #body>
        <ActivityForm
          :activity="editingActivity"
          :loading="activitiesStore.loading"
          @submit="handleSubmit"
          @cancel="closeModal"
        />
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useActivitiesStore } from '@/stores'
import { useToast } from '@/composables'
import { Button, Loading, EmptyState } from '@/components/common'
import { ActivityCard, ActivityForm } from '@/components/activities'
import { Icon } from '@/components/icons'
import Modal from '@/components/Modal.vue'
import type { Activity, CreateActivityRequest, UpdateActivityRequest } from '@/types'

const router = useRouter()
const activitiesStore = useActivitiesStore()
const { success, error } = useToast()

const showModal = ref(false)
const editingActivity = ref<Activity | null>(null)

const activities = computed(() => activitiesStore.sortedActivities)

onMounted(async () => {
  try {
    await activitiesStore.fetchActivities()
  } catch (e) {
    error('Nepavyko gauti veiklų')
  }
})

function openCreateModal() {
  editingActivity.value = null
  showModal.value = true
}

function editActivity(activity: Activity) {
  editingActivity.value = activity
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingActivity.value = null
}

async function handleSubmit(data: CreateActivityRequest | UpdateActivityRequest) {
  try {
    if (editingActivity.value) {
      const updated = await activitiesStore.updateActivity(editingActivity.value.id, data as UpdateActivityRequest)
      if (updated) {
        success('Veikla atnaujinta!')
        closeModal()
      } else {
        error(activitiesStore.error || 'Nepavyko atnaujinti veiklos')
      }
    } else {
      const created = await activitiesStore.createActivity(data as CreateActivityRequest)
      if (created) {
        success('Veikla sukurta!')
        closeModal()
      } else {
        error(activitiesStore.error || 'Nepavyko sukurti veiklos')
      }
    }
  } catch (e) {
    error('Nepavyko išsaugoti veiklos')
  }
}

async function handleDelete(id: number) {
  if (!confirm('Ar tikrai norite ištrinti šią veiklą?')) return
  
  try {
    await activitiesStore.deleteActivity(id)
    success('Veikla ištrinta')
  } catch (e) {
    error('Nepavyko ištrinti veiklos')
  }
}
</script>

<style scoped>
.activities-page {
  padding: 2rem 1rem;
  min-height: calc(100vh - 120px);
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
}

.activities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .activities-grid {
    grid-template-columns: 1fr;
  }
}
</style>
