<template>
  <div class="landing-page">
    <section class="hero-section">
      <div class="container">
        <div class="hero-content">
          <h1 class="hero-title">
            <span class="gradient-text">Cirends</span>
            <br />
            Organizuokite veiklas, valdykite užduotis ir išlaidas
          </h1>
          <p class="hero-description">
            Modernus grupių veiklų valdymo įrankis, skirtas paprastam užduočių planavimui, 
            dalyvių koordinavimui ir bendromis išlaidomis dalijimui.
          </p>
          <div class="hero-actions">
            <Button variant="primary" size="lg" @click="$router.push('/register')">
              Registruotis dabar
            </Button>
            <Button variant="secondary" size="lg" @click="$router.push('/login')">
              Prisijungti
            </Button>
          </div>
        </div>
      </div>
    </section>

    <section class="features-section">
      <div class="container">
        <h2 class="section-title">Funkcijos</h2>
        <div class="features-grid">
          <Card class="feature-card">
            <Icon name="calendar" size="48" class="feature-icon" />
            <h3>Veiklų valdymas</h3>
            <p>Kurkite veiklas, nustatykite datas, vietą ir aprašymus. Kvieškite dalyvius ir valdykite jų prieigą.</p>
          </Card>

          <Card class="feature-card">
            <Icon name="check" size="48" class="feature-icon" />
            <h3>Užduočių plnavimas</h3>
            <p>Priskirkite užduotis dalyviams, sekite jų progresą ir būseną. Organizuokite darbus efektyviai.</p>
          </Card>

          <Card class="feature-card">
            <Icon name="people" size="48" class="feature-icon" />
            <h3>Dalyvių valdymas</h3>
            <p>Kvieškite narius el. paštu, valdykite jų prieigą ir vykdykite pakeitimus bet kuriuo metu.</p>
          </Card>

          <Card class="feature-card">
            <Icon name="attach-money" size="48" class="feature-icon" />
            <h3>Išlaidų skirstymas</h3>
            <p>Registruokite bendras išlaidas, automatiškai paskirstykite sumas tarp dalyvių ir sekite mokėjimus.</p>
          </Card>

          <Card class="feature-card">
            <Icon name="dashboard" size="48" class="feature-icon" />
            <h3>Statistika</h3>
            <p>Peržiūrėkite veiklų, užduočių ir išlaidų ataskaitas. Sekite, kas sumokėjo, o kas dar skolingas.</p>
          </Card>

          <Card class="feature-card">
            <Icon name="info" size="48" class="feature-icon" />
            <h3>Saugumas</h3>
            <p>Jūsų duomenys saugūs. Kiekvienas dalyvis mato tik tai, prie ko turi prieigą.</p>
          </Card>
        </div>
      </div>
    </section>

    <section class="how-it-works-section">
      <div class="container">
        <h2 class="section-title">Kaip tai veikia?</h2>
        <div class="steps">
          <div class="step">
            <div class="step-number">1</div>
            <h3>Sukurkite veiklą</h3>
            <p>Registruokitės ir sukurkite pirmą veiklą - išvyką, šventę, ar bet kokį kitą renginį.</p>
          </div>

          <div class="step">
            <div class="step-number">2</div>
            <h3>Pakvieskite dalyvius</h3>
            <p>Nusiųskite pakvietimus pagal el. paštą kitiems dalyviams. Jie automatiškai gaus prieigą prie veiklos.</p>
          </div>

          <div class="step">
            <div class="step-number">3</div>
            <h3>Planuokite užduotis</h3>
            <p>Sukurkite užduočių sąrašą, priskirkite jas dalyviams ir sekite įgyvendinimą.</p>
          </div>

          <div class="step">
            <div class="step-number">4</div>
            <h3>Tvarkykite išlaidas</h3>
            <p>Registruokite išlaidas, paskirstykite sumas ir sekite, kas sumokėjo. Sistema automatiškai apskaičiuoja balansus.</p>
          </div>

          <div class="step">
            <div class="step-number">5</div>
            <h3>Stebėkite progresą</h3>
            <p>Peržiūrėkite veiklos būseną, užduočių įvykdymą ir išlaidų apžvalgą vienoje vietoje.</p>
          </div>

          <div class="step">
            <div class="step-number">6</div>
            <h3>Mėgaukitės veikla</h3>
            <p>Leiskitės į veiklą be rūpesčių, žinodami, kad visi organizaciniai darbai yra tvarkomi.</p>
          </div>


        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { Button, Card } from '@/components/common'
import { Icon } from '@/components/icons'
</script>

<style scoped>
.landing-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #f9fafb 0%, #ffffff 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

/* Hero Section */
.hero-section {
  padding: 4rem 0;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-title {
  font-size: 3rem;
  font-weight: 800;
  line-height: 1.2;
  margin-bottom: 1.5rem;
  color: var(--text-primary);
}

.gradient-text {
  background: var(--primary-gradient);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero-description {
  font-size: 1.25rem;
  line-height: 1.8;
  color: var(--text-secondary);
  margin-bottom: 2.5rem;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

/* Features Section */
.features-section {
  padding: 4rem 0;
  background: white;
}

.section-title {
  font-size: 2.5rem;
  font-weight: 700;
  text-align: center;
  margin-bottom: 3rem;
  color: var(--text-primary);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
}

.feature-card {
  text-align: center;
  padding: 2rem;
  transition: var(--transition);
}

.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  margin-bottom: 1rem;
  color: var(--primary);
}

.feature-card h3 {
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: var(--text-primary);
}

.feature-card p {
  color: var(--text-secondary);
  line-height: 1.6;
}

/* How It Works Section */
.how-it-works-section {
  padding: 4rem 0;
  background: var(--gray-50);
}

.steps {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

.step {
  text-align: center;
}

.step-number {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: var(--primary-gradient);
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}

.step h3 {
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: var(--text-primary);
}

.step p {
  color: var(--text-secondary);
  line-height: 1.6;
}

/* CTA Section */
.cta-section {
  padding: 4rem 0;
  background: var(--primary-gradient);
  color: white;
}

.cta-content {
  text-align: center;
  max-width: 700px;
  margin: 0 auto;
}

.cta-content h2 {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 1rem;
}

.cta-content p {
  font-size: 1.25rem;
  margin-bottom: 2rem;
  opacity: 0.95;
}

/* Responsive */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2rem;
  }

  .hero-description {
    font-size: 1rem;
  }

  .section-title {
    font-size: 1.75rem;
  }

  .hero-actions {
    flex-direction: column;
  }

  .features-grid {
    grid-template-columns: 1fr;
  }

  .steps {
    grid-template-columns: 1fr;
  }

  .cta-content h2 {
    font-size: 1.75rem;
  }

  .cta-content p {
    font-size: 1rem;
  }
}
</style>
