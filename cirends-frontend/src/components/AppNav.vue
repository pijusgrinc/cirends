<template>
  <nav class="app-nav" :class="{ open: mobileMenuOpen }">
    <button class="hamburger" @click="toggleMenu">
      <span></span>
      <span></span>
      <span></span>
    </button>

    <ul class="nav-menu" :class="{ active: mobileMenuOpen }">
      <li class="nav-item">
        <router-link to="/dashboard" @click="closeMenu" class="nav-link">
          <Icon name="dashboard" class="nav-icon" />
          Pagrindinis
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/activities" @click="closeMenu" class="nav-link">
          <Icon name="activities" class="nav-icon" />
          Veiklos
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/invitations" @click="closeMenu" class="nav-link">
          <Icon name="invitations" class="nav-icon" />
          Kvietimai
          <span v-if="pendingInvitations > 0" class="badge badge-danger">{{ pendingInvitations }}</span>
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/profile" @click="closeMenu" class="nav-link">
          <Icon name="users" class="nav-icon" />
          Profilis
        </router-link>
      </li>
      <li v-if="authStore.isAdmin" class="nav-item">
        <router-link to="/admin" @click="closeMenu" class="nav-link">
          <Icon name="admin" class="nav-icon" />
          Admin
        </router-link>
      </li>
    </ul>
  </nav>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { invitationsAPI, getToken } from '../api.js'
import { useAuthStore } from '@/stores/auth'
import { Icon } from '@/components/icons'

const authStore = useAuthStore()
const mobileMenuOpen = ref(false)
const pendingInvitations = ref(0)

onMounted(async () => {
  if (getToken()) {
    try {
      const result = await invitationsAPI.getPending()
      if (result.ok) {
        pendingInvitations.value = result.data.length || 0
      }
    } catch (error) {
      console.error('Klaida gaunant kvietimus:', error)
    }
  }
})

const toggleMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

const closeMenu = () => {
  mobileMenuOpen.value = false
}
</script>

<style scoped>
.app-nav {
  background-color: var(--white);
  border-bottom: 1px solid var(--border-color);
  position: sticky;
  top: 0;
  z-index: 90;
}

.nav-menu {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  max-width: 1200px;
  margin: 0 auto;
}

.nav-item {
  flex: 1;
  position: relative;
}

.nav-link {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 1rem;
  text-decoration: none;
  color: var(--text-primary);
  font-weight: 500;
  font-size: 0.95rem;
  transition: var(--transition);
  border-bottom: 3px solid transparent;
  white-space: nowrap;
  position: relative;
}

.nav-link:hover {
  background-color: var(--gray-50);
  border-bottom-color: var(--primary);
  color: var(--primary);
}

.nav-link.router-link-active {
  background-color: var(--primary-light);
  border-bottom-color: var(--primary);
  color: var(--primary);
}

.nav-icon {
  width: 20px;
  height: 20px;
}

.hamburger {
  display: none;
  flex-direction: column;
  background: none;
  border: none;
  cursor: pointer;
  padding: 1rem;
  gap: 5px;
}

.hamburger span {
  width: 25px;
  height: 3px;
  background-color: var(--text-primary);
  border-radius: 2px;
  transition: var(--transition);
}

.hamburger:active span:nth-child(1) {
  transform: rotate(-45deg) translate(-8px, 8px);
}

.hamburger:active span:nth-child(2) {
  opacity: 0;
}

.hamburger:active span:nth-child(3) {
  transform: rotate(45deg) translate(-8px, -8px);
}

.badge {
  position: absolute;
  top: 0;
  right: 0;
  font-size: 0.7rem;
  padding: 0.2rem 0.4rem;
}

@media (max-width: 768px) {
  .app-nav {
    position: relative;
    top: auto;
  }

  .hamburger {
    display: flex;
  }

  .nav-menu {
    display: none;
    flex-direction: column;
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background-color: white;
    box-shadow: var(--box-shadow-lg);
    z-index: 1000;
    animation: slideDown 0.3s ease;
  }

  .nav-menu.active {
    display: flex;
  }

  .nav-item {
    flex: none;
    border-bottom: 1px solid var(--gray-300);
  }

  .nav-link {
    justify-content: flex-start;
    padding: 1.2rem 1rem;
    border-bottom: none;
    border-left: 3px solid transparent;
  }

  .nav-link:hover,
  .nav-link.router-link-active {
    border-bottom: none;
    border-left-color: var(--primary);
  }
}
</style>
