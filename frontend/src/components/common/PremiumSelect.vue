<template>
  <div class="premium-select-wrapper" :class="{ 'is-open': isOpen }" v-click-outside="closeDropdown">
    <div class="premium-select-display" @click="toggleDropdown" :class="{ 'has-value': modelValue }">
      <div class="selected-content">
        <div v-if="selectedOption?.icon" class="option-icon" v-html="selectedOption.icon"></div>
        <span v-if="selectedOption" class="selected-text">{{ selectedOption.label }}</span>
        <span v-else class="placeholder-text">{{ placeholder || 'Sélectionner...' }}</span>
      </div>
      
      <svg class="dropdown-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="6 9 12 15 18 9"></polyline>
      </svg>
    </div>

    <transition name="dropdown-anim">
      <div v-if="isOpen" class="premium-select-menu">
        <div class="options-container">
          <div 
            v-for="option in normalizedOptions" 
            :key="option.value"
            class="premium-option"
            :class="{ 'is-selected': modelValue === option.value }"
            @click="selectOption(option)"
          >
            <div class="option-content-inner">
              <div v-if="option.icon" class="option-icon" v-html="option.icon"></div>
              <span class="option-label">{{ option.label }}</span>
            </div>
            <svg v-if="modelValue === option.value" class="check-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="20 6 9 17 4 12"></polyline>
            </svg>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
export default {
  name: 'PremiumSelect',
  props: {
    modelValue: {
      type: [String, Number, Boolean],
      default: ''
    },
    options: {
      type: Array,
      required: true,
      // Can be an array of strings/numbers or objects { label, value }
    },
    placeholder: {
      type: String,
      default: ''
    }
  },
  emits: ['update:modelValue', 'change'],
  data() {
    return {
      isOpen: false
    }
  },
  computed: {
    normalizedOptions() {
      if (!this.options || this.options.length === 0) return []
      
      // Handle array of strings/numbers
      if (typeof this.options[0] !== 'object') {
        return this.options.map(opt => ({
          label: String(opt),
          value: opt
        }))
      }
      
      // Handle array of objects
      return this.options
    },
    selectedOption() {
      return this.normalizedOptions.find(opt => opt.value === this.modelValue)
    }
  },
  methods: {
    toggleDropdown() {
      this.isOpen = !this.isOpen
    },
    closeDropdown() {
      this.isOpen = false
    },
    selectOption(option) {
      if (this.modelValue !== option.value) {
        this.$emit('update:modelValue', option.value)
        this.$emit('change', option.value)
      }
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
.premium-select-wrapper {
  position: relative;
  width: 100%;
  min-width: 180px;
  user-select: none;
}

/* Display Button - uses theme variables that auto-switch */
.premium-select-display {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 14px 18px;
  background: var(--r-main-bg);
  border: 1.5px solid var(--r-border);
  border-radius: 14px;
  color: var(--r-text-main);
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-sizing: border-box;
}

.premium-select-display:hover,
.premium-select-wrapper.is-open .premium-select-display {
  border-color: var(--accent);
  background: var(--card-bg);
  box-shadow: 0 10px 25px -5px var(--accent-soft);
}


.selected-content {
  display: flex;
  align-items: center;
  gap: 12px;
  overflow: hidden;
}

.selected-text {
  color: var(--r-text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.placeholder-text {
  color: var(--r-text-sub);
}

.dropdown-icon {
  width: 16px;
  height: 16px;
  color: var(--r-text-sub);
  transition: transform 0.3s ease;
  flex-shrink: 0;
}

.premium-select-wrapper.is-open .dropdown-icon {
  transform: rotate(-180deg);
  color: var(--primary, #0ea5e9);
}


/* Dropdown Menu */
.premium-select-menu {
  position: absolute;
  top: calc(100% + 10px);
  left: 0;
  width: 100%;
  background: var(--card-bg);
  backdrop-filter: blur(12px);
  border: 1.5px solid rgba(0, 0, 0, 0.1);
  border-radius: 20px;
  box-shadow: 0 20px 50px -15px rgba(0, 0, 0, 0.15);
  z-index: 9999;
  overflow: hidden;
  transform-origin: top center;
}

.options-container {
  max-height: 280px;
  overflow-y: auto;
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.options-container::-webkit-scrollbar { width: 4px; }
.options-container::-webkit-scrollbar-track { background: transparent; }
.options-container::-webkit-scrollbar-thumb { background: var(--r-border); border-radius: 4px; }

.premium-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 12px 16px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  color: var(--r-text-main);
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-sizing: border-box;
}

.option-content-inner {
  display: flex;
  align-items: center;
  gap: 12px;
}

.option-icon {
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  opacity: 0.8;
}

.option-icon :deep(svg) {
  width: 100%;
  height: 100%;
}

.premium-option:hover {
  background: var(--accent-soft);
  color: var(--accent);
  padding-left: 20px;
}

.premium-option.is-selected {
  background: var(--accent-grad);
  color: white;
  font-weight: 800;
}

.check-icon {
  width: 14px;
  height: 14px;
  color: white;
}


.dropdown-anim-enter-active,
.dropdown-anim-leave-active {
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.dropdown-anim-enter-from,
.dropdown-anim-leave-to {
  opacity: 0;
  transform: translateY(-8px) scaleY(0.95);
}
</style>

