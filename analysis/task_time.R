# Load necessary libraries
library(readxl)
library(car)
library(gridExtra)
library(lmerTest)
library(dplyr)
library(afex)
library(tidyr)
library(ez)

# Load the data from Excel file using a absolute path
file_path <- "Task_Time_Accuracy.xlsx"
df <- read_excel(file_path)

# Convert relevant variables to factors
df$Condition <- as.factor(df$Condition)
df$Visualization <- as.factor(df$Visualization)


# Normality Check Function
check_normality <- function(data, variable) {
  
  # Shapiro-Wilk Test
  shapiro_test <- shapiro.test(data[[variable]])
  shapiro_p <- shapiro_test$p.value
  
  
  return(shapiro_p)
}

# Perform the normality check for TaskCompletionTime
p_task_completion_time <- check_normality(df, 'TaskCompletionTime')
print(paste("Shapiro-Wilk Test p-value for TaskCompletionTime:", round(p_task_completion_time, 3)))

# Levene's Test for Homogeneity of Variance
levene_test <- leveneTest(TaskCompletionTime ~ Condition, data = df)
print(paste("Levene’s Test p-value:", round(levene_test$`Pr(>F)`[1], 3)))

# Fit your model
model <- lmer(TaskCompletionTime ~ Condition * Visualization + (1 | PairID), data = df)

# Perform ANOVA and get p-values
anova_results <- anova(model)
print(anova_results)

anova_results$part.eta.sq = with(anova_results, (`F value` * `NumDF`) / (`F value` * `NumDF` + `DenDF`))

print(anova_results$part.eta.sq)

