import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { Fish } from '../types/fish';
import { fishService } from '../services/fishService';

interface FishState {
  items: Fish[];
  selectedFish: Fish | null;
  loading: boolean;
  error: string | null;
}

const initialState: FishState = {
  items: [],
  selectedFish: null,
  loading: false,
  error: null,
};

export const fetchAllFish = createAsyncThunk(
  'fish/fetchAll',
  async () => {
    const response = await fishService.getAllFish();
    return response;
  }
);

export const fetchFishById = createAsyncThunk(
  'fish/fetchById',
  async (id: number) => {
    const response = await fishService.getFishById(id);
    return response;
  }
);

export const fetchFishBySpecies = createAsyncThunk(
  'fish/fetchBySpecies',
  async (species: string) => {
    const response = await fishService.getFishBySpecies(species);
    return response;
  }
);

const fishSlice = createSlice({
  name: 'fish',
  initialState,
  reducers: {
    clearSelectedFish: (state) => {
      state.selectedFish = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch all fish
      .addCase(fetchAllFish.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllFish.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchAllFish.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch fish';
      })

      // Fetch fish by id
      .addCase(fetchFishById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchFishById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedFish = action.payload;
      })
      .addCase(fetchFishById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch fish';
      })

      // Fetch fish by species
      .addCase(fetchFishBySpecies.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchFishBySpecies.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchFishBySpecies.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch fish by species';
      });
  },
});

export const { clearSelectedFish } = fishSlice.actions;
export default fishSlice.reducer;