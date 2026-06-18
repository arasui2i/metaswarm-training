import { Box, Drawer, List, ListItemButton, ListItemText, Toolbar, Typography } from '@mui/material';
import { Outlet, useNavigate } from 'react-router-dom';

const DRAWER_WIDTH = 220;

export function AppLayout() {
  const navigate = useNavigate();
  return (
    <Box sx={{ display: 'flex' }}>
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' },
        }}
      >
        <Toolbar>
          <Typography variant="h6">CRM</Typography>
        </Toolbar>
        <List>
          <ListItemButton onClick={() => navigate('/leads')}>
            <ListItemText primary="Leads" />
          </ListItemButton>
          <ListItemButton onClick={() => navigate('/accounts')}>
            <ListItemText primary="Accounts" />
          </ListItemButton>
        </List>
      </Drawer>
      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Outlet />
      </Box>
    </Box>
  );
}
