<template>
  <div class="empty-page">
    <div class="container">
      <div class="empty-state">
        <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/><path d="M12 8v4m0 4v.01"/>
        </svg>
        <h1>404 - Puslapis nerastas</h1>
        <p>Atsiprašau, šis puslapis nėra pasiekiamas.</p>
        <router-link to="/" class="btn btn-primary">Grįžti į pradžią</router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.empty-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 1rem;
}

.container {
  max-width: 600px;
  margin: 0 auto;
  width: 100%;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: var(--border-radius);
  box-shadow: var(--box-shadow);
}

.empty-icon {
  width: 120px;
  height: 120px;
  margin: 0 auto 2rem;
  color: var(--primary);
  opacity: 0.3;
}

.empty-state h1 {
  color: var(--dark);
  margin-bottom: 1rem;
}

.empty-state p {
  color: var(--gray-600);
  font-size: 1.05rem;
  margin-bottom: 2rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.75rem 2rem;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  text-decoration: none;
  transition: var(--transition);
  font-size: 1rem;
}

.btn-primary {
  background: var(--primary-gradient);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}
</style>
