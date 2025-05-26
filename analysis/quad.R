library(readxl)
library(ARTool)
library(dplyr)
library(ggplot2)

#https://rcompanion.org/handbook/F_16.html
cat("\014")

# Load the data from Excel file using a absolute path
df <- read.csv("quadrant_time_proportions.csv")

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
df$ParticipantID <- factor(df$ParticipantID)
df$PairID <- factor(df$PairID)


df$NearTime <- as.numeric(df$NearTime)
df$FarTime <- as.numeric(df$FarTime)
df$LeftTime <- as.numeric(df$LeftTime)
df$RightTime <- as.numeric(df$RightTime)


# List of dependent variables
dependent_vars <- c("NearTime", "LeftTime", "RightTime")
#dependent_vars <- c("FarTime") #use ART

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
  model <- lmer(as.formula(paste(model_var, "~ Condition * Visualization  + (1 | PairID)")), data = df)
  
  # Perform ANOVA and get p-values
  anova_results <- anova(model)
  print(anova_results)
  
  # Calculate partial eta squared
  anova_results$part.eta.sq <- with(anova_results, (`F value` * `NumDF`) / (`F value` * `NumDF` + `DenDF`))
  print(paste("Partial Eta Squared for", var, ":", round(anova_results$part.eta.sq, 3)))
  
  # Perform the ART analysis
  #art_result <- art(as.formula(paste(model_var, "~ Condition * Visualization + (1|PairID)")), data = df)
  
  # Perform ANOVA on the ART results
  #anova_results <- anova(art_result)
  #print(anova_results)
  
  # Calculate partial eta squared
  #anova_results$part.eta.sq <- with(anova_results, `F` * `Df` / (`F` * `Df` + `Df.res`))
  #print(paste("Partial Eta Squared for", var, ":", round(anova_results$part.eta.sq, 3)))
  
}