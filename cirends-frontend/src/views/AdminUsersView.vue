<template>
  <div class="admin-users-page">
    <div class="container">
      <div class="page-header">
        <div>
          <h1>Naudotojų valdymas</h1>
          <p class="subtitle">Valdykite sistemos naudotojus ir jų teises</p>
        </div>
        <router-link to="/admin" class="back-button">
          <Icon name="arrow-back" :size="20" />
          Atgal
        </router-link>
      </div>

      <Loading v-if="loading" />
      
      <div v-else-if="error" class="error-message">
        <Icon name="warning" :size="20" />
        {{ error }}
      </div>

      <Card v-else-if="users.length" class="users-table-card">
        <div class="table-wrapper">
          <table class="users-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Vardas</th>
                <th>El. paštas</th>
                <th>Rolė</th>
                <th>Būsena</th>
                <th>Sukurta</th>
                <th>Veiksmai</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="u in users" :key="u.id">
                <td><span class="user-id">#{{ u.id }}</span></td>
                <td>
                  <div class="user-name">
                    <div class="user-avatar">{{ u.name.charAt(0).toUpperCase() }}</div>
                    {{ u.name }}
                  </div>
                </td>
                <td class="email-cell">{{ u.email }}</td>
                <td>
                  <select 
                    :value="u.role" 
                    @change="onRoleChange(u, $event)" 
                    :disabled="saving"
                    class="role-select"
                  >
                    <option value="Admin">Admin</option>
                    <option value="User">User</option>
                  </select>
                </td>
                <td>
                  <label class="switch">
                    <input 
                      type="checkbox" 
                      :checked="u.isActive" 
                      @change="toggleActive(u)" 
                      :disabled="saving" 
                    />
                    <span class="slider"></span>
                  </label>
                  <span class="status-label" :class="{ active: u.isActive }">
                    {{ u.isActive ? 'Aktyvus' : 'Neaktyvus' }}
                  </span>
                </td>
                <td class="date-cell">{{ formatDate(u.createdAt) }}</td>
                <td>
                  <Button 
                    variant="danger" 
                    size="sm" 
                    @click="remove(u)" 
                    :disabled="saving"
                  >
                    <Icon name="delete" :size="16" />
                    Šalinti
                  </Button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </Card>

      <div v-else class="empty-state">
        <Icon name="users" :size="48" />
        <p>Naudotojų nerasta</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useAdminStore } from '@/stores'
import { Button, Card, Loading } from '@/components/common'
import { Icon } from '@/components/icons'

const admin = useAdminStore()
const loading = computed(() => admin.loading)
const error = computed(() => admin.error)
const users = computed(() => admin.users)
const saving = ref(false)

function formatDate(s: string) {
  const d = new Date(s)
  return isNaN(d.getTime()) ? '' : d.toLocaleDateString('lt-LT')
}

onMounted(async () => {
  await admin.fetchAllUsers()
})

async function onRoleChange(u: any, e: Event) {
  const role = (e.target as HTMLSelectElement).value
  saving.value = true
  const ok = await admin.updateUserRole(u.id, role)
  if (!ok) {
    await admin.fetchAllUsers()
  }
  saving.value = false
}

async function toggleActive(u: any) {
  saving.value = true
  const ok = await admin.toggleUserActive(u.id)
  if (!ok) {
    await admin.fetchAllUsers()
  }
  saving.value = false
}

async function remove(u: any) {
  if (!confirm(`Ar tikrai šalinti naudotoją ${u.name}?`)) return
  saving.value = true
  await admin.deleteUser(u.id)
  saving.value = false
}
</script>

<style scoped>
.admin-users-page {
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
  align-items: flex-start;
  margin-bottom: 2rem;
  gap: 1rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.subtitle {
  color: var(--text-secondary);
  font-size: 0.95rem;
}

.back-button {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  background: white;
  color: var(--text-primary);
  border: 1px solid var(--border);
  border-radius: var(--border-radius);
  text-decoration: none;
  font-weight: 500;
  transition: var(--transition);
}

.back-button:hover {
  background: var(--gray-50);
  border-color: var(--primary);
  color: var(--primary);
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  background: var(--danger-light);
  color: var(--danger);
  border-radius: var(--border-radius);
  border-left: 4px solid var(--danger);
}

.users-table-card {
  overflow: hidden;
}

.table-wrapper {
  overflow-x: auto;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table thead {
  background: var(--gray-50);
}

.users-table th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 2px solid var(--border);
}

.users-table td {
  padding: 1rem;
  border-bottom: 1px solid var(--border);
  color: var(--text-primary);
}

.users-table tbody tr:hover {
  background: var(--gray-50);
}

.users-table tbody tr:last-child td {
  border-bottom: none;
}

.user-id {
  font-family: 'Roboto Mono', monospace;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.user-name {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 500;
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.875rem;
  flex-shrink: 0;
}

.email-cell {
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.role-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: var(--border-radius);
  background: white;
  color: var(--text-primary);
  font-size: 0.9rem;
  cursor: pointer;
  transition: var(--transition);
}

.role-select:hover {
  border-color: var(--primary);
}

.role-select:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.role-select:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
  vertical-align: middle;
  margin-right: 0.5rem;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--gray-300);
  transition: 0.3s;
  border-radius: 24px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: 0.3s;
  border-radius: 50%;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

input:checked + .slider {
  background-color: var(--success);
}

input:checked + .slider:before {
  transform: translateX(20px);
}

input:disabled + .slider {
  opacity: 0.5;
  cursor: not-allowed;
}

.status-label {
  font-size: 0.875rem;
  color: var(--text-secondary);
  vertical-align: middle;
}

.status-label.active {
  color: var(--success);
  font-weight: 500;
}

.date-cell {
  color: var(--text-secondary);
  font-size: 0.9rem;
  white-space: nowrap;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--text-secondary);
}

.empty-state svg {
  color: var(--gray-400);
  margin-bottom: 1rem;
}

.empty-state p {
  font-size: 1.125rem;
  margin: 0;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }

  .back-button {
    align-self: flex-start;
  }

  .table-wrapper {
    overflow-x: scroll;
  }

  .users-table {
    min-width: 800px;
  }
}
</style>
