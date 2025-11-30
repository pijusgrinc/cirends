<script setup>
import { ref } from 'vue'
import { Icon } from '@/components/icons'

const mobileMenuOpen = ref(false)

const toggleMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
}
</script>

<template>
  <nav class="navbar">
    <button class="hamburger" @click="toggleMenu" :class="{ active: mobileMenuOpen }">
      <span></span>
      <span></span>
      <span></span>
    </button>
    
    <ul class="nav-menu" :class="{ active: mobileMenuOpen }">
      <li class="nav-item">
        <router-link to="/activities" @click="closeMobileMenu">
          <Icon name="activities" class="nav-icon" />
          Veiklos
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/tasks" @click="closeMobileMenu">
          <Icon name="check" class="nav-icon" />
          Užduotys
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/expenses" @click="closeMobileMenu">
          <Icon name="attach-money" class="nav-icon" />
          Išlaidos
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/invitations" @click="closeMobileMenu">
          <Icon name="invitations" class="nav-icon" />
          Kvietimai
        </router-link>
      </li>
      <li class="nav-item">
        <router-link to="/my-expenses" @click="closeMobileMenu">
          <Icon name="expenses" class="nav-icon" />
          Mano išlaidos
        </router-link>
      </li>
    </ul>
  </nav>
</template>


<style scoped>
.navbar {
  background: white;
  border-bottom: 1px solid var(--border);
  padding: 0 1rem;
  grid-column: 1;
  grid-row: 2;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.nav-menu {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  gap: 0;
}

.nav-item {
  flex: 1;
}

.nav-item a {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1rem;
  text-decoration: none;
  color: var(--text-primary);
  font-weight: 500;
  transition: all 0.3s ease;
  border-bottom: 3px solid transparent;
  font-size: 0.95rem;
}

.nav-item a:hover {
  background-color: var(--gray-50);
  border-bottom-color: var(--primary);
  color: var(--primary);
}

.nav-item a.router-link-active {
  border-bottom-color: var(--primary);
  color: var(--primary);
  background-color: rgba(102, 126, 234, 0.05);
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
  padding: 1rem 0;
}

.hamburger span {
  width: 25px;
  height: 3px;
  background-color: var(--text-primary);
  margin: 5px 0;
  transition: all 0.3s ease;
  border-radius: 2px;
}

/* Hamburger animacija */
.hamburger.active span:nth-child(1) {
  transform: rotate(-45deg) translate(-8px, 8px);
}

.hamburger.active span:nth-child(2) {
  opacity: 0;
}

.hamburger.active span:nth-child(3) {
  transform: rotate(45deg) translate(-8px, -8px);
}

@media (max-width: 768px) {
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
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    z-index: 1000;
  }

  .nav-menu.active {
    display: flex;
    animation: slideDown 0.3s ease;
  }

  .nav-item {
    border-bottom: 1px solid #e0e0e0;
  }

  .nav-item a {
    justify-content: flex-start;
    padding: 1.2rem 1rem;
    border-bottom: none;
    border-left: 3px solid transparent;
  }

  .nav-item a:hover {
    border-left-color: #667eea;
    border-bottom: none;
  }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>