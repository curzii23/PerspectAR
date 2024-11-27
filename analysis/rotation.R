library(readxl)
library(ARTool)
library(dplyr)
library(ggplot2)

#https://rcompanion.org/handbook/F_16.html
cat("\014")

# Load the data from Excel file using a absolute path
df <- read.csv("euler_angles_statistics_by_task_per_participant_condition.csv")

# Check and prepare your data
head(df)
str(df)

# Normality Check Function
check_normality <- function(data, variable) {
  
  # Shapiro-Wilk Test
  shapiro_test <- shapiro.test(data[[variable]])
  shapiro_p <- shapiro_test$p.value
  
  return(shapiro_p)
}

# Convert columns to appropriate types
df$Condition <- factor(df$Condition)
df$Visualization <- factor(df$Visualization)
df$Dataset <- factor(df$Dataset)
df$PairID <- factor(df$PairID)

#mean_yaw, mean_pitch, mean_roll
df$mean_yaw <- as.numeric(df$mean_yaw)  # Ensure mean_yaw is numeric
df$mean_pitch <- as.numeric(df$mean_pitch)  # Ensure mean_pitch is numeric
df$mean_roll <- as.numeric(df$mean_roll)  # Ensure mean_roll is numeric

# List of dependent variables
dependent_vars <- c("mean_yaw", "mean_pitch", "mean_roll")

# Loop over each dependent variable
for (var in dependent_vars) {

  df[[var]] <- as.numeric(df[[var]])
  
  # Perform the normality check
  p_value <- check_normality(df, var)
  print(paste("Shapiro-Wilk Test p-value for", var, ":", round(p_value, 3)))
  
  # If the p-value is less than 0.05, perform a log transformation
  if (p_value < 0.05) {
    log_var <- paste("Log", var, sep = "")
    df[[log_var]] <- log(df[[var]] + 1)  # Adding 1 to avoid log(0)
    
    # Check normality of the log-transformed variable
    p_log_value <- check_normality(df, log_var)
    print(paste("Shapiro-Wilk Test p-value for", log_var, ":", round(p_log_value, 3)))
    
    # Use the log-transformed variable in the model
    model_var <- log_var
  } else {
    # Use the original variable in the model
    model_var <- var
  }
  
  # Perform linear mixed-effects model
  model <- lmer(as.formula(paste(model_var, "~ Condition * Visualization * Dataset + (1 | PairID)")), data = df)
  
  # Perform ANOVA and get p-values
  anova_results <- anova(model)
  print(anova_results)
  
  # Calculate partial eta squared
  anova_results$part.eta.sq <- with(anova_results, (`F value` * `NumDF`) / (`F value` * `NumDF` + `DenDF`))
  print(paste("Partial Eta Squared for", var, ":", round(anova_results$part.eta.sq, 3)))
}



