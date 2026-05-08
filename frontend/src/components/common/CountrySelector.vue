<template>
  <div class="country-selector-wrapper" v-click-outside="closeDropdown">
    <div class="country-display-glass" @click="toggleDropdown" :class="{ 'is-open': isOpen }">
      <img :src="`https://flagcdn.com/w40/${selectedCountry.iso.toLowerCase()}.png`" class="country-flag-main" :alt="selectedCountry.iso">
      <span class="country-code-main">{{ selectedCountry.code }}</span>
      <svg class="dropdown-chevron" :class="{ 'rotate': isOpen }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="6 9 12 15 18 9"/></svg>
    </div>

    <transition name="dropdown-anim">
      <div v-if="isOpen" class="country-dropdown-menu">
        <div class="search-box-elite">
          <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input 
            ref="searchInput"
            v-model="searchQuery" 
            type="text" 
            :placeholder="$t('common.search') || 'Rechercher...'"
            @click.stop
          >
        </div>

        <div class="country-options-list">
          <div 
            v-for="country in filteredCountries" 
            :key="country.iso"
            class="country-option-item"
            :class="{ 'is-selected': modelValue === country.code && selectedCountry.iso === country.iso }"
            @click="selectCountry(country)"
          >
            <div class="option-left">
              <img :src="`https://flagcdn.com/w40/${country.iso.toLowerCase()}.png`" class="option-flag" :alt="country.iso">
              <span class="option-name">{{ country.name }}</span>
            </div>
            <span class="option-code">{{ country.code }}</span>
          </div>
          
          <div v-if="filteredCountries.length === 0" class="no-results-elite">
            {{ $t('common.noResults') || 'Aucun résultat' }}
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
import countriesList from '@/assets/data/countries.json'

export default {
  name: 'CountrySelector',
  props: {
    modelValue: { type: String, default: '+216' },
    selectedIso: { type: String, default: 'tn' }
  },
  emits: ['update:modelValue', 'update:selectedIso', 'change'],
  data() {
    return {
      isOpen: false,
      searchQuery: '',
      countries: countriesList
    }
  },
  computed: {
    selectedCountry() {
      return this.countries.find(c => c.iso.toLowerCase() === this.selectedIso.toLowerCase()) || this.countries[0]
    },
    filteredCountries() {
      if (!this.searchQuery) return this.countries
      const query = this.searchQuery.toLowerCase()
      return this.countries.filter(c => 
        c.name.toLowerCase().includes(query) || 
        c.code.includes(query) ||
        c.iso.toLowerCase().includes(query)
      )
    }
  },
  methods: {
    toggleDropdown() {
      this.isOpen = !this.isOpen
      if (this.isOpen) {
        this.$nextTick(() => {
          this.$refs.searchInput?.focus()
        })
      }
    },
    closeDropdown() {
      this.isOpen = false
      this.searchQuery = ''
    },
    selectCountry(country) {
      this.$emit('update:modelValue', country.code)
      this.$emit('update:selectedIso', country.iso)
      this.$emit('change', country)
      this.closeDropdown()
    }
  },
  directives: {
    'click-outside': {
      mounted(el, binding) {
        el.clickOutsideEvent = (event) => {
          if (!(el === event.target || el.contains(event.target))) {
            binding.value(event)
          }
        }
        document.body.addEventListener('click', el.clickOutsideEvent)
      },
      unmounted(el) {
        document.body.removeEventListener('click', el.clickOutsideEvent)
      }
    }
  }
}
</script>

<style scoped>
.country-selector-wrapper {
  position: relative;
  width: 130px;
  flex-shrink: 0;
  user-select: none;
}

.country-display-glass {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 16px;
  height: 52px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
  box-sizing: border-box;
}

.country-display-glass:hover,
.country-display-glass.is-open {
  border-color: var(--accent-color);
  background: var(--card-bg);
  box-shadow: 0 10px 25px -5px var(--accent-soft);
}

.country-flag-main {
  width: 24px;
  height: 16px;
  object-fit: cover;
  border-radius: 3px;
  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}

.country-code-main {
  font-size: 15px;
  font-weight: 800;
  color: var(--r-text-main);
  letter-spacing: -0.2px;
}

.dropdown-chevron {
  width: 14px;
  height: 14px;
  margin-left: auto;
  color: var(--r-text-sub);
  transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.dropdown-chevron.rotate {
  transform: rotate(-180deg);
  color: var(--accent-color);
}

/* DROPDOWN MENU */
.country-dropdown-menu {
  position: absolute;
  top: calc(100% + 12px);
  left: 0;
  width: 320px;
  background: var(--glass-bg);
  backdrop-filter: blur(20px);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.2);
  z-index: 1000;
  overflow: hidden;
  transform-origin: top left;
}

.search-box-elite {
  position: relative;
  padding: 16px;
  border-bottom: 1px solid var(--r-border);
}

.search-icon {
  position: absolute;
  left: 28px;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  color: var(--accent-color);
}

.search-box-elite input {
  width: 100%;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 12px;
  padding: 10px 15px 10px 40px;
  color: var(--r-text-main);
  font-size: 14px;
  font-weight: 500;
  outline: none;
  transition: all 0.3s;
}

.search-box-elite input:focus {
  border-color: var(--accent-color);
  background: var(--card-bg);
  box-shadow: 0 0 15px var(--accent-soft);
}

.country-options-list {
  max-height: 300px;
  overflow-y: auto;
  padding: 8px;
}

.country-options-list::-webkit-scrollbar { width: 4px; }
.country-options-list::-webkit-scrollbar-thumb { background: var(--accent-color); border-radius: 10px; }

.country-option-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.country-option-item:hover {
  background: var(--accent-soft);
  color: var(--accent-color);
}

.country-option-item.is-selected {
  background: var(--accent-soft);
}

.option-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.option-flag {
  width: 24px;
  height: 16px;
  object-fit: cover;
  border-radius: 3px;
}

.option-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--r-text-main);
}

.option-code {
  font-size: 13px;
  font-weight: 700;
  color: var(--accent-color);
}

.no-results-elite {
  padding: 20px;
  text-align: center;
  color: var(--r-text-sub);
  font-size: 14px;
}

/* ANIMATIONS */
.dropdown-anim-enter-active,
.dropdown-anim-leave-active {
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.dropdown-anim-enter-from,
.dropdown-anim-leave-to {
  opacity: 0;
  transform: translateY(-20px) scale(0.95);
}
</style>
